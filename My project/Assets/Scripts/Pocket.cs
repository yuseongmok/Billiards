using UnityEngine;

public class Pocket : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ball"))
        {
            Destroy(other.gameObject);

            GameManager.Instance.ScorePoint(GameManager.Instance.currentTurn);
        }
        else if (other.CompareTag("WhiteBall"))
        {
            Debug.Log("파울");
            other.transform.position = new Vector3(0, 0.5f, -3f);
            other.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
            other.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        }
    }
}