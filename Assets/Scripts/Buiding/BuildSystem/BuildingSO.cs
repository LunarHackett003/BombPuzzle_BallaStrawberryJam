using UnityEngine;
using System.Threading.Tasks;


#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "BuildingSO", menuName = "Scriptable Objects/BuildingSO")]
public class BuildingSO : ScriptableObject
{
    [SerializeField] internal Texture2D icon;
    public string DisplayName;
    public GameObject prefab;
    string _prefabNameLast;

    public bool refreshAsset;

#if UNITY_EDITOR
    async void GetIcon()
    {
        if (prefab != null)
        {

            if (refreshAsset || prefab.name != _prefabNameLast)
            {
                _prefabNameLast = prefab.name;
                var tex = AssetPreview.GetAssetPreview(prefab);
                string path = AssetDatabase.GetAssetPath(prefab) + "_icon.png";
                await Task.Delay(300);
                tex = AssetPreview.GetAssetPreview(prefab);
                icon = new(tex.width, tex.height, textureFormat:(TextureFormat)27, mipChain:false);
                Graphics.CopyTexture(tex, icon);
                System.IO.File.WriteAllBytes(path, tex.EncodeToPNG());
                icon = AssetDatabase.LoadAssetAtPath<Texture2D>(path);

                refreshAsset = false;
            }
        }
    }
#endif
    private void OnValidate()
    {
#if UNITY_EDITOR
        GetIcon();
#endif

    }










}
