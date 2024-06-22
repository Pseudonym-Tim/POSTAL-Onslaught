using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base class for all entities...
/// </summary>
public class Entity : MonoBehaviour
{
    private void Awake() => OnEntityAwake();
    private void OnDestroy() => OnEntityDestroyed();
    private void Update() => OnEntityUpdate();

    public virtual void OnEntityAwake() { }
    public virtual void OnEntitySpawn() { }
    protected virtual void OnEntityUpdate() { }
    protected virtual void OnEntityDestroyed() { }
    public virtual void OnLevelGenerated() { }
    public virtual void OnNavmeshBuilt() { }

    public void DestroyEntity(float destroyTime = 0)
    {
        if(Application.isPlaying) { Destroy(EntityObject, destroyTime); }
        else { DestroyImmediate(EntityObject); }
    }

    public void ShowEntity(bool showEntity = true)
    {
        EntityObject.SetActive(showEntity);
    }

    public void SetParent(Transform parentTransform, bool worldPositionStays = true)
    {
        EntityTransform.SetParent(parentTransform, worldPositionStays);
    }

    protected void SetupEntityAnim()
    {
        EntityAnim = new EntityAnimator(this);
    }

    private void OnDrawGizmos() => OnDrawEntityGizmos();

    protected virtual void OnDrawEntityGizmos()
    {

    }

    public virtual Vector3 CenterOfMass { get { return EntityPosition; } }
    public EntityAnimator EntityAnim { get; set; } = null;
    public EntityData EntityData { get; set; } = null;
    public GameObject EntityObject { get { return gameObject; } }
    public Transform EntityTransform { get { return transform; } }
    public Vector3 EntityPosition { get { return EntityTransform.position; } set { EntityTransform.position = value; } }
    public Vector3 EntityLocalPosition { get { return EntityTransform.localPosition; } set { EntityTransform.localPosition = value; } }
    public Quaternion EntityRotation { get { return EntityTransform.rotation; } set { EntityTransform.rotation = value; } }
    public Vector3 EntityEulerAngles { get { return EntityTransform.eulerAngles; } set { EntityTransform.eulerAngles = value; } }
    public Vector3 EntityLocalEulerAngles { get { return EntityTransform.localEulerAngles; } set { EntityTransform.localEulerAngles = value; } }
    public Quaternion EntityLocalRotation { get { return EntityTransform.localRotation; } set { EntityTransform.localRotation = value; } }
    public Vector3 EntityLocalScale { get { return EntityTransform.localScale; } set { EntityTransform.localScale = value; } }
}
