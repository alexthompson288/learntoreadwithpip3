using UnityEngine;
using System.Collections;

// TODO: After you rework the blackboard scripts and prefabs, make the blackboard colliders block the CollectionRoomBoard collider and delete this class
public class DisableCollectionRoomCollider : Singleton<DisableCollectionRoomCollider> 
{
	public void EnableCollider ()
	{
		collider.enabled = true;
	}
	
	public void DisableCollider () 
	{
		collider.enabled = false;
	}
}
