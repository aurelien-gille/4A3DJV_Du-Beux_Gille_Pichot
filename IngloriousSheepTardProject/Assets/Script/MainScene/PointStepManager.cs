using UnityEngine;
using System.Collections;

public class PointStepManager : MonoBehaviour
{
    #region Properties
    [SerializeField]
    public PointStepManager NextPoint;

    [SerializeField]
    public Transform myself;

    public bool isAttacking;
    #endregion

    #region Public Methods
    public void DestroySelf()
    {
        Destroy(myself.gameObject);
    }
    public void DestroyChild()
    {
        if (NextPoint)
        {
            NextPoint.DestroyChild();
            NextPoint.DestroySelf();
        }
    }
    #endregion

}
