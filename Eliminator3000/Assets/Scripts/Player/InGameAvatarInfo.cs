using UnityEngine;

public class InGameAvatarInfo : MonoBehaviour
{
    [TextArea(15, 20), SerializeField]
    private string Info;

    public int PlayerID = -1;

}
