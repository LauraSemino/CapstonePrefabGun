using Unity.VisualScripting;
using UnityEngine;

public class DoubleDoor : MonoBehaviour
{
    [SerializeField] float timeToOpen;
    [SerializeField] GameObject doorL;
    [SerializeField] GameObject doorR;
    [SerializeField] float doorSpeed;

    // Update is called once per frame
    void Update()
    {
        if(timeToOpen > 0)
        {
            timeToOpen -= Time.deltaTime;
        }

        if (timeToOpen <= 0 && timeToOpen >= -5)
        {
            doorL.transform.position -= new Vector3(Time.deltaTime, 0, 0);
            doorR.transform.position += new Vector3(Time.deltaTime, 0, 0);
            timeToOpen -= Time.deltaTime;
        }
    }
}
