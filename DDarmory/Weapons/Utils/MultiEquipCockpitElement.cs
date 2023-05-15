using UnityEngine;
using UnityEngine.Events;

public class MultiEquipCockpitElement : MonoBehaviour
{
    public GameObject cockpitObject;

    public Vector3 cockpitPosition;

    public Vector3 cockpitRotation;

    public static UnityEvent<GameObject> OnCreateCockpitObject = new UnityEvent<GameObject>();

    public virtual void Start()
    {
        var root = transform.root;

        if (!root.Find(cockpitObject.name))
        {
            var cockpitObjectName = cockpitObject.name;
            var obj = Instantiate(cockpitObject, root);
            
            obj.transform.localPosition = cockpitPosition;
            obj.transform.localRotation = Quaternion.Euler(cockpitRotation);
            
            obj.name = cockpitObjectName;
            
            obj.SetActive(true);
            OnCreateCockpitObject.Invoke(obj);
        }
    }
}