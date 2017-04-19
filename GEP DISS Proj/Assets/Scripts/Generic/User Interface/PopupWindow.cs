using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopupWindow : MonoBehaviour
{
    protected float offsetX = 0.0f;
    protected float offsetY = 0.0f;

    public virtual void CloseWindow()
    {
        GlobalGEPSettings.gameStatus = GlobalGEPSettings.GAME_STATE.RUNNING;
        InputHandler.camControlsEnabled = true;
        Destroy(gameObject);
    }

    public void OnBeginDrag()
    {
        offsetX = transform.position.x - Input.mousePosition.x;
        offsetY = transform.position.y - Input.mousePosition.y;
    }

    public void OnDrag()
    {
        Vector3 newPosition = new Vector3(offsetX + Input.mousePosition.x, offsetY + Input.mousePosition.y, transform.position.z);
        transform.position = newPosition;
    }
}
