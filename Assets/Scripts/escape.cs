using UnityEngine;
using TMPro;
public class Finish : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text escapedText;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Rigidbody rigidbody = other.GetComponent<Rigidbody>();

            if (rigidbody != null)
            {
                rigidbody.linearVelocity = Vector3.zero;
                rigidbody.isKinematic = true;
            }

            escapedText.gameObject.SetActive(true);
        }
    }
}