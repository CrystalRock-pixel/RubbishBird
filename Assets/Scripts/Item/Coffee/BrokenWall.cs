using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrokenWall : MonoBehaviour, IInteractive
{
    public Transform instance { get => this.transform; set => throw new System.NotImplementedException(); }
    public bool Disposable { get => true; set => throw new System.NotImplementedException(); }
    private Player player;

    public Sprite brokenSprite;
    private BoxCollider boxCollider;

    public Vector3 dialogOffset = new Vector3(4, 2.5f, 0);
    public bool Interact()
    {
        player = Player.Instance;
        if (player.GetCurrentInteractiveItem() != null)
        {
            IInteractive interacitveItem = player.GetCurrentInteractiveItem();
            string interactiveItemName = interacitveItem.instance.name;
            if (interactiveItemName.Contains("DiamondPickaxe"))
            {
                player.OverInteractive();
                //Debug.Log("破墙中...");
                DialogManager.Instance.ShowDialog("破墙中...", transform.position, dialogOffset, this.transform);
                Destroy(interacitveItem.instance.gameObject);
                StartCoroutine(BreakWall());
                return true;
            }
            else
            {
                //Debug.Log("需要钻石镐才能破坏这堵墙！");
                DialogManager.Instance.ShowDialog("你有种预感  它跟黑曜石一样硬", transform.position, dialogOffset, this.transform);
                return false;
            }
        }
        else
        {
            //Debug.Log("这是一堵看起来很坚固的墙");
            DialogManager.Instance.ShowDialog("这扇窗看样子可以打破了  \n 但用你脆弱的鸟头可不是好主意", transform.position, dialogOffset, this.transform);
            return false;
        }
    }

    IEnumerator BreakWall()
    {
        yield return new WaitForSeconds(5f);
        boxCollider = transform.GetChild(0).GetComponent<BoxCollider>();
        GetComponent<SpriteRenderer>().sprite = brokenSprite;
        boxCollider.enabled = false;
        //Destroy(this.gameObject);
    }

    public void InteractEnd()
    {
        throw new System.NotImplementedException();
    }
}
