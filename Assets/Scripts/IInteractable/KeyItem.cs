using UnityEngine;

public class KeyItem : Item
{
    public override void Start(){
        inventoryManager = InventoryManager.Instance;
        if(inventoryManager.HasItem("Key")) Destroy(gameObject);
    }
}
