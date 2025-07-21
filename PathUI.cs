using UnityEngine;
using UnityEngine.UI;

public class PathUI : MonoBehaviour
{
    public InputField startInput;
    public InputField endInput;
    public GridBuilder gridBuilder;

    public void OnCalculatePath()
    {
        string[] startParts = startInput.text.Split(',');
        string[] endParts = endInput.text.Split(',');

        int startX = int.Parse(startParts[0]);
        int startZ = int.Parse(startParts[1]);
        int endX = int.Parse(endParts[0]);
        int endZ = int.Parse(endParts[1]);

        gridBuilder.CalculatePath(startX, startZ, endX, endZ);
    }
}
