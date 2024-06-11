using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using static UnityEngine.GraphicsBuffer;

[AddComponentMenu("TuanThanh/Parallax")]
public class Parallax : MonoBehaviour
{
    public Transform player;
    public bool lockYParallax = true;
    public bool autoParallax = true;
    public float adjustAutoY = 1;
    [ConditionalHide("autoParallax", true)]
    public float parallaxFactorX, parallaxFactorY;
    public bool infiniteLoopX;
    public bool infiniteLoopY;

    private Camera cam;
    private Vector3 lastCamPos;
    private float startZ, initialYDiff;
    private Vector3 travel => cam.transform.position - lastCamPos;

    private Vector2 parallaxFactor;

    private float textureUnitSizeX;
    private float textureUnitSizeY;

    float delayPrint = 2f, nextPrint = 0f;

    private void Start()
    {
        cam = Camera.main;
        lastCamPos = cam.transform.position;
        startZ = transform.position.z;
        initialYDiff = transform.position.y - cam.transform.position.y;

        float visibleRangeInFrontPlayer = Mathf.Abs(cam.transform.position.z - player.position.z - cam.nearClipPlane);
        float visibleRangeBehindPlayer = cam.farClipPlane - visibleRangeInFrontPlayer;
        float fromLayerToPlayer = transform.position.z - player.position.z;
        float autoFactor = fromLayerToPlayer > 0 ? (fromLayerToPlayer / visibleRangeBehindPlayer) : (fromLayerToPlayer / visibleRangeInFrontPlayer);

        if (autoParallax)
        {
            parallaxFactor.x = autoFactor;
            parallaxFactor.y = autoFactor * adjustAutoY;
        }
        else
        {
            parallaxFactor = new Vector2(parallaxFactorX, parallaxFactorY);
        }

        Sprite sprite = GetComponent<SpriteRenderer>().sprite;
        Texture2D texture = sprite.texture;
        textureUnitSizeX = texture.width / sprite.pixelsPerUnit;
        textureUnitSizeY = texture.height / sprite.pixelsPerUnit;
    }

    private void Update()
    {
        transform.position += new Vector3(travel.x * parallaxFactor.x, travel.y * parallaxFactor.y);
        lastCamPos = cam.transform.position;

        if (lockYParallax)
        {
            transform.position = new Vector3(transform.position.x, cam.transform.position.y + initialYDiff, startZ);
        }

        if (infiniteLoopX)
        {
            if (Mathf.Abs(cam.transform.position.x - transform.position.x) >= textureUnitSizeX)
            {
                float offsetPositionX = (cam.transform.position.x - transform.position.x) % textureUnitSizeX;
                transform.position = new Vector3(cam.transform.position.x + offsetPositionX, transform.position.y, startZ);
            }
        }

        if (infiniteLoopY)
        {
            if (Mathf.Abs(cam.transform.position.y - transform.position.y) >= textureUnitSizeY)
            {
                float offsetPositionY = (cam.transform.position.y - transform.position.y) % textureUnitSizeY;
                transform.position = new Vector3(transform.position.x, cam.transform.position.y + offsetPositionY, startZ);
            }
        }
    }

    void DelayPrint()
    {
        if (Time.time >= nextPrint)
        {
            Debug.Log("Tranform Pos: "+transform.position);
            nextPrint = Time.time + delayPrint;
        }
    }
}

public class ConditionalHideAttribute : PropertyAttribute
{
    public string ConditionalSourceField = "";
    public bool HideInInspector = false;

    public ConditionalHideAttribute(string conditionalSourceField, bool hideInInspector)
    {
        this.ConditionalSourceField = conditionalSourceField;
        this.HideInInspector = hideInInspector;
    }
}

[CustomPropertyDrawer(typeof(ConditionalHideAttribute))]
public class ConditionalHidePropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        ConditionalHideAttribute condHAtt = (ConditionalHideAttribute)attribute;
        bool enabled = GetConditionalHideAttributeResult(condHAtt, property);

        bool wasEnabled = GUI.enabled;
        GUI.enabled = enabled;
        if (!condHAtt.HideInInspector || enabled)
        {
            EditorGUI.PropertyField(position, property, label, true);
        }

        GUI.enabled = wasEnabled;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        ConditionalHideAttribute condHAtt = (ConditionalHideAttribute)attribute;
        bool enabled = GetConditionalHideAttributeResult(condHAtt, property);

        if (!condHAtt.HideInInspector || enabled)
        {
            return EditorGUI.GetPropertyHeight(property, label);
        }
        else
        {
            return -EditorGUIUtility.standardVerticalSpacing;
        }
    }

    private bool GetConditionalHideAttributeResult(ConditionalHideAttribute condHAtt, SerializedProperty property)
    {
        bool enabled = true;
        string propertyPath = property.propertyPath;
        string conditionPath = propertyPath.Replace(property.name, condHAtt.ConditionalSourceField);
        SerializedProperty sourcePropertyValue = property.serializedObject.FindProperty(conditionPath);

        if (sourcePropertyValue != null)
        {
            enabled = !sourcePropertyValue.boolValue;
        }
        else
        {
            Debug.LogWarning($"Attempting to use a ConditionalHideAttribute but no matching SourcePropertyValue found in object: {condHAtt.ConditionalSourceField}");
        }

        return enabled;
    }
}
