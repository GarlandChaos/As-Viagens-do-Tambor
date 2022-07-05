using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardSpace : MonoBehaviour
{
    public List<BoardSpace> adjacent = new List<BoardSpace>();
    public int id = -1;
    public GameObject marker = null;
    //Renderer spaceRenderer = null;
    //MaterialPropertyBlock materialPropertyBlock = null;

    //private void Awake()
    //{
    //    spaceRenderer = GetComponent<Renderer>();
    //    materialPropertyBlock = new MaterialPropertyBlock();
    //}

    //public void SetSelection()
    //{
    //    StartCoroutine(AnimateSelection(1));
    //}

    //void SetSelectionPropertyBlock(int value)
    //{
    //    spaceRenderer.GetPropertyBlock(materialPropertyBlock);
    //    materialPropertyBlock.SetInt("_Outline", value);
    //    spaceRenderer.SetPropertyBlock(materialPropertyBlock);
    //}

    //IEnumerator AnimateSelection(int selected)
    //{
    //    WaitForEndOfFrame wait = new WaitForEndOfFrame();
    //    float timer = 0f;
    //    float selectionDuration = 0.5f;

    //    //SetSelectionPropertyBlock(selected);

    //    while (timer < 1f)
    //    {
    //        timer += Time.deltaTime / selectionDuration;
            
    //        yield return wait;
    //    }

    //    selected = (selected == 0) ? 1 : 0;

    //    StartCoroutine(AnimateSelection(selected));
    //}
}
