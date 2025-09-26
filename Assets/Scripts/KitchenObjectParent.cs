using UnityEngine;

public class KitchenObjectParent : MonoBehaviour, IKitchenObjectParent
{
    private KitchenObject kitchenObject;

    public Transform GetKitchenObjectFollowTransform()
    {
        return this.transform; // The plate will follow this point
    }

    public void SetKitchenObject(KitchenObject kitchenObject)
    {
        this.kitchenObject = kitchenObject;
    }

    public void ClearKitchenObject()
    {
        kitchenObject = null;
    }

    public bool HasKitchenObject()
    {
        return kitchenObject != null;
    }

    KitchenObject IKitchenObjectParent.GetKitchenObject()
    {
        throw new System.NotImplementedException();
    }
}
