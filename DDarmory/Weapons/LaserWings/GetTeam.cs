using UnityEngine;

public class GetTeam : MonoBehaviour
{
    public string GetMyTeam()
    {
        _actor = gameObject.GetComponentInParent<Actor>();
        if (_actor == null) Debug.Log("GetMyTeam: Actor is null");

        var team = _actor.team;

        string result;
        if (team == Teams.Allied)
        {
            bluFor.SetActive(true);
            opFor.SetActive(false);
            result = "Allied";
        }
        else
        {
            if (team == Teams.Enemy)
            {
                bluFor.SetActive(false);
                opFor.SetActive(true);
                result = "Enemy";
            }
            else
            {
                result = "Null";
            }
        }

        return result;
    }

    public GameObject bluFor;

    public GameObject opFor;

    private Actor _actor;
}