using UnityEngine;
using System.Collections;

public interface IPlayer {

	void tryToMoveTo(Vector3 pos);

	void shoot(Vector3 pos);
}