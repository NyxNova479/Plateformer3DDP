using UnityEngine;

public class VoidBehaviour : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.Instance.ResetScore();
            other.transform.position = new Vector3(3, 1, 5);
        }
    }
}
