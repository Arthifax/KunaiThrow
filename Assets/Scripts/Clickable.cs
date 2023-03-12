using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Clickable : MonoBehaviour
{
   [SerializeField] private Reticle reticleManager;
   [SerializeField] private NinjaGameManager nGameManager;

   private void OnMouseDown()
   {
      if (!nGameManager.hasFiredProjectile)
      {
         reticleManager.Selected(this.gameObject);
      }
   }

   private void OnMouseUp()
   {
      if (reticleManager.selectedObject != null)
      {
         reticleManager.Deselect();
      }
   }

}
