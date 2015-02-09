using UnityEngine;
using System.Collections;

public abstract class APlayerScript : MonoBehaviour
{

    #region Properties
    public bool isAttacking;
    public float timeAttack;
    public TypePlayer type;
    #endregion

    #region Public Methods
    abstract public void tryToMoveTo (Vector3 pos);

    abstract public bool tryToAttack();
    abstract public void stopAttack();
    #endregion
}
