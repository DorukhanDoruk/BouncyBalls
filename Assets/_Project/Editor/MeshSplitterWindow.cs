using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace Editor
{
    public class MeshSplitterWindow : EditorWindow
    {
        private struct Tri
        {
            public Vector3 A, B, C;
            public Vector3 Na, Nb, Nc;
        }

        private Mesh _source;
        private int _pieceCount = 6;
        private int _seed = 1;
        private int _relaxPasses = 2;
        private string _outputFolder = "Assets/_Project/Runtime/Art/Models/Generated";

        [MenuItem("BouncyBalls/Mesh Splitter")]
        private static void Open()
        {
            GetWindow<MeshSplitterWindow>("Mesh Splitter");
        }

        private void OnGUI()
        {
            _source = (Mesh)EditorGUILayout.ObjectField("Source Mesh", _source, typeof(Mesh), false);
            _pieceCount = EditorGUILayout.IntSlider("Pieces", _pieceCount, 2, 24);
            _seed = EditorGUILayout.IntField("Seed", _seed);
            _relaxPasses = EditorGUILayout.IntSlider("Relax Passes", _relaxPasses, 0, 8);
            _outputFolder = EditorGUILayout.TextField("Output Folder", _outputFolder);

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                "Voronoi fracture. Seeds are scattered through the mesh; each piece is the mesh " +
                "clipped against the bisector planes to its neighbours, and every cut is capped, " +
                "so the pieces come out as closed solids with flat break faces.\n\n" +
                "Change the seed to re-roll until you like the break. Relax passes even out the " +
                "piece sizes; 0 leaves them wildest.\n\n" +
                "Assumes a convex source (a disc or a ball is fine) and needs Read/Write enabled.",
                MessageType.Info);

            using (new EditorGUI.DisabledScope(_source == null))
            {
                if (GUILayout.Button("Fracture"))
                {
                    Fracture();
                }
            }
        }

        private void Fracture()
        {
            var vertices = _source.vertices;
            var normals = _source.normals;
            var triangles = _source.triangles;

            var source = new List<Tri>(triangles.Length / 3);
            for (int t = 0; t < triangles.Length; t += 3)
            {
                int i0 = triangles[t], i1 = triangles[t + 1], i2 = triangles[t + 2];
                source.Add(new Tri
                {
                    A = vertices[i0], B = vertices[i1], C = vertices[i2],
                    Na = normals[i0], Nb = normals[i1], Nc = normals[i2],
                });
            }

            var seeds = ScatterSeeds(source);

            Directory.CreateDirectory(_outputFolder);

            int written = 0;
            for (int i = 0; i < seeds.Length; i++)
            {
                var piece = BuildPiece(i, seeds, source);
                if (piece.Count == 0)
                {
                    Debug.LogWarning($"[MeshSplitter] seed {i} produced nothing, skipped.");
                    continue;
                }

                Save(piece, i);
                written++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[MeshSplitter] wrote {written} piece(s) of {_source.name} to {_outputFolder}");
        }

        // A piece is the whole mesh cut back by the bisector plane to every other seed.
        private static List<Tri> BuildPiece(int index, Vector3[] seeds, List<Tri> source)
        {
            var piece = new List<Tri>(source);
            var cut = new List<Vector3>();

            for (int j = 0; j < seeds.Length; j++)
            {
                if (j == index)
                {
                    continue;
                }

                Vector3 delta = seeds[j] - seeds[index];
                float length = delta.magnitude;
                if (length < 1e-6f)
                {
                    continue;
                }

                Vector3 normal = delta / length;
                float offset = Vector3.Dot(normal, (seeds[index] + seeds[j]) * 0.5f);

                cut.Clear();
                piece = ClipToPlane(piece, normal, offset, cut);
                if (piece.Count == 0)
                {
                    break;
                }

                AddCap(piece, cut, normal);
            }

            return piece;
        }

        // Sutherland-Hodgman per triangle. Keeps the negative side and reports the new edge
        // points, which are what the cap is then built from.
        private static List<Tri> ClipToPlane(List<Tri> input, Vector3 normal, float offset, List<Vector3> cut)
        {
            var output = new List<Tri>(input.Count);

            var point = new Vector3[3];
            var pointNormal = new Vector3[3];
            var distance = new float[3];

            var polyPoint = new List<Vector3>(4);
            var polyNormal = new List<Vector3>(4);

            foreach (var tri in input)
            {
                point[0] = tri.A; point[1] = tri.B; point[2] = tri.C;
                pointNormal[0] = tri.Na; pointNormal[1] = tri.Nb; pointNormal[2] = tri.Nc;

                for (int i = 0; i < 3; i++)
                {
                    distance[i] = Vector3.Dot(normal, point[i]) - offset;
                }

                if (distance[0] <= 0f && distance[1] <= 0f && distance[2] <= 0f)
                {
                    output.Add(tri);
                    continue;
                }

                if (distance[0] >= 0f && distance[1] >= 0f && distance[2] >= 0f)
                {
                    continue;
                }

                polyPoint.Clear();
                polyNormal.Clear();

                for (int i = 0; i < 3; i++)
                {
                    int next = (i + 1) % 3;
                    bool inside = distance[i] <= 0f;
                    bool nextInside = distance[next] <= 0f;

                    if (inside)
                    {
                        polyPoint.Add(point[i]);
                        polyNormal.Add(pointNormal[i]);
                    }

                    if (inside != nextInside)
                    {
                        float t = distance[i] / (distance[i] - distance[next]);
                        Vector3 hit = Vector3.Lerp(point[i], point[next], t);

                        polyPoint.Add(hit);
                        polyNormal.Add(Vector3.Lerp(pointNormal[i], pointNormal[next], t).normalized);
                        cut.Add(hit);
                    }
                }

                for (int i = 1; i + 1 < polyPoint.Count; i++)
                {
                    output.Add(new Tri
                    {
                        A = polyPoint[0], B = polyPoint[i], C = polyPoint[i + 1],
                        Na = polyNormal[0], Nb = polyNormal[i], Nc = polyNormal[i + 1],
                    });
                }
            }

            return output;
        }

        // The cut cross-section of a convex solid is a convex polygon, so sorting the edge
        // points around their middle and fanning them closes the piece.
        private static void AddCap(List<Tri> tris, List<Vector3> cut, Vector3 normal)
        {
            var points = new List<Vector3>();
            foreach (var candidate in cut)
            {
                bool duplicate = false;
                foreach (var kept in points)
                {
                    if ((kept - candidate).sqrMagnitude < 1e-8f)
                    {
                        duplicate = true;
                        break;
                    }
                }

                if (!duplicate)
                {
                    points.Add(candidate);
                }
            }

            if (points.Count < 3)
            {
                return;
            }

            Vector3 center = Vector3.zero;
            foreach (var p in points)
            {
                center += p;
            }

            center /= points.Count;

            Vector3 axis = Mathf.Abs(normal.y) < 0.9f ? Vector3.up : Vector3.right;
            Vector3 u = Vector3.Cross(normal, axis).normalized;
            Vector3 v = Vector3.Cross(normal, u);

            points.Sort((a, b) => Angle(a - center, u, v).CompareTo(Angle(b - center, u, v)));

            for (int i = 1; i + 1 < points.Count; i++)
            {
                Vector3 a = points[0];
                Vector3 b = points[i];
                Vector3 c = points[i + 1];

                // Unity derives a face normal from cross(b - a, c - a), so flip if it faces inward.
                if (Vector3.Dot(Vector3.Cross(b - a, c - a), normal) < 0f)
                {
                    (b, c) = (c, b);
                }

                tris.Add(new Tri { A = a, B = b, C = c, Na = normal, Nb = normal, Nc = normal });
            }
        }

        private static float Angle(Vector3 offset, Vector3 u, Vector3 v)
        {
            return Mathf.Atan2(Vector3.Dot(offset, v), Vector3.Dot(offset, u));
        }

        private Vector3[] ScatterSeeds(List<Tri> source)
        {
            var random = new System.Random(_seed);
            var bounds = _source.bounds;

            var seeds = new Vector3[_pieceCount];
            for (int i = 0; i < _pieceCount; i++)
            {
                seeds[i] = new Vector3(
                    Mathf.Lerp(bounds.min.x, bounds.max.x, (float)random.NextDouble()),
                    Mathf.Lerp(bounds.min.y, bounds.max.y, (float)random.NextDouble()),
                    Mathf.Lerp(bounds.min.z, bounds.max.z, (float)random.NextDouble()));
            }

            var centroids = new Vector3[source.Count];
            for (int t = 0; t < source.Count; t++)
            {
                centroids[t] = (source[t].A + source[t].B + source[t].C) / 3f;
            }

            // Lloyd relaxation: pull every seed to the middle of what it owns, so the break
            // gives comparable chunks instead of slivers next to huge pieces.
            for (int pass = 0; pass < _relaxPasses; pass++)
            {
                var sum = new Vector3[_pieceCount];
                var count = new int[_pieceCount];

                foreach (var centroid in centroids)
                {
                    int nearest = Nearest(centroid, seeds);
                    sum[nearest] += centroid;
                    count[nearest]++;
                }

                for (int i = 0; i < _pieceCount; i++)
                {
                    if (count[i] > 0)
                    {
                        seeds[i] = sum[i] / count[i];
                    }
                }
            }

            return seeds;
        }

        private static int Nearest(Vector3 point, Vector3[] seeds)
        {
            float best = float.MaxValue;
            int bestSeed = 0;

            for (int i = 0; i < seeds.Length; i++)
            {
                float distance = (point - seeds[i]).sqrMagnitude;
                if (distance < best)
                {
                    best = distance;
                    bestSeed = i;
                }
            }

            return bestSeed;
        }

        private void Save(List<Tri> tris, int index)
        {
            // Cut faces are flat and the surface is smooth, so no vertex is shared between
            // triangles; welding them would average the two apart.
            var vertices = new List<Vector3>(tris.Count * 3);
            var normals = new List<Vector3>(tris.Count * 3);
            var indices = new List<int>(tris.Count * 3);

            foreach (var tri in tris)
            {
                if (Vector3.Cross(tri.B - tri.A, tri.C - tri.A).sqrMagnitude < 1e-12f)
                {
                    continue;
                }

                indices.Add(vertices.Count);
                indices.Add(vertices.Count + 1);
                indices.Add(vertices.Count + 2);

                vertices.Add(tri.A); vertices.Add(tri.B); vertices.Add(tri.C);
                normals.Add(tri.Na); normals.Add(tri.Nb); normals.Add(tri.Nc);
            }

            var mesh = new Mesh { name = $"{_source.name}_Piece_{index}" };
            mesh.SetVertices(vertices);
            mesh.SetNormals(normals);
            mesh.SetTriangles(indices, 0);
            mesh.RecalculateBounds();

            AssetDatabase.CreateAsset(mesh, $"{_outputFolder}/{mesh.name}.asset");
        }
    }
}
