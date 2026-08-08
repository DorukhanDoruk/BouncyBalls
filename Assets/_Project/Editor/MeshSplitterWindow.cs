using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
namespace Editor
{
    public class MeshSplitterWindow : EditorWindow
    {
        private Mesh _source;
        private int _pieceCount = 6;
        private string _outputFolder = "Assets/_Project/Runtime/Art/Models/Generated";

        [MenuItem("BouncyBalls/Mesh Splitter")]
        private static void Open()
        {
            GetWindow<MeshSplitterWindow>("Mesh Splitter");
        }

        private void OnGUI()
        {
            _source = (Mesh)EditorGUILayout.ObjectField("Source Mesh", _source, typeof(Mesh), false);
            _pieceCount = EditorGUILayout.IntSlider("Pieces", _pieceCount, 2, 16);
            _outputFolder = EditorGUILayout.TextField("Output Folder", _outputFolder);

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                "Triangles are sorted into wedges by the angle of their centroid around Y. " +
                "Nothing is cut, so the wedge edges are only as straight as the source tessellation. " +
                "The source mesh must have Read/Write enabled.",
                MessageType.Info);

            using (new EditorGUI.DisabledScope(_source == null))
            {
                if (GUILayout.Button("Split"))
                {
                    Split();
                }
            }
        }

        private void Split()
        {
            var vertices = _source.vertices;
            var normals = _source.normals;
            var triangles = _source.triangles;

            var pieceVertices = new List<Vector3>[_pieceCount];
            var pieceNormals = new List<Vector3>[_pieceCount];
            var pieceTriangles = new List<int>[_pieceCount];

            // Source index -> piece index, so a shared vertex is copied once per piece.
            var remap = new Dictionary<int, int>[_pieceCount];

            for (int i = 0; i < _pieceCount; i++)
            {
                pieceVertices[i] = new List<Vector3>();
                pieceNormals[i] = new List<Vector3>();
                pieceTriangles[i] = new List<int>();
                remap[i] = new Dictionary<int, int>();
            }

            float wedge = Mathf.PI * 2f / _pieceCount;

            for (int t = 0; t < triangles.Length; t += 3)
            {
                Vector3 centroid =
                    (vertices[triangles[t]] + vertices[triangles[t + 1]] + vertices[triangles[t + 2]]) / 3f;

                float angle = Mathf.Atan2(centroid.x, centroid.z);
                if (angle < 0f)
                {
                    angle += Mathf.PI * 2f;
                }

                int piece = Mathf.Min((int)(angle / wedge), _pieceCount - 1);

                for (int corner = 0; corner < 3; corner++)
                {
                    int sourceIndex = triangles[t + corner];
                    if (!remap[piece].TryGetValue(sourceIndex, out int mapped))
                    {
                        mapped = pieceVertices[piece].Count;
                        remap[piece][sourceIndex] = mapped;

                        pieceVertices[piece].Add(vertices[sourceIndex]);
                        pieceNormals[piece].Add(normals[sourceIndex]);
                    }

                    pieceTriangles[piece].Add(mapped);
                }
            }

            Directory.CreateDirectory(_outputFolder);

            int written = 0;
            for (int i = 0; i < _pieceCount; i++)
            {
                if (pieceTriangles[i].Count == 0)
                {
                    Debug.LogWarning($"[MeshSplitter] wedge {i} caught no triangles, skipped.");
                    continue;
                }

                var mesh = new Mesh { name = $"{_source.name}_Piece_{i}" };
                mesh.SetVertices(pieceVertices[i]);
                mesh.SetNormals(pieceNormals[i]);
                mesh.SetTriangles(pieceTriangles[i], 0);
                mesh.RecalculateBounds();

                AssetDatabase.CreateAsset(mesh, $"{_outputFolder}/{mesh.name}.asset");
                written++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[MeshSplitter] wrote {written} piece(s) of {_source.name} to {_outputFolder}");
        }
    }
}
