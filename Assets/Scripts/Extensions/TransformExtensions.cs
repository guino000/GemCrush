using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TransformExtensions
{
    public static IEnumerator Move(this Transform t, Vector3 target, float duration)
    {
        t.gameObject.GetComponent<GridItem>().isMoving = true;
        Debug.Log(string.Format("Moving {0}{1} to {2}{3}", t.position.x, t.position.y, target.x, target.y));
        Vector3 diffVector = (target - t.position);
        float diffLenght = diffVector.magnitude;
        diffVector.Normalize();
        float counter = 0;
        while(counter < duration)
        {
            float movAmount = (Time.deltaTime * diffLenght) / duration;
            t.position += diffVector * movAmount;
            counter += Time.deltaTime;
            yield return null;
        }

        t.position = target;
        yield return null;
        Debug.Log(string.Format("Moved {0}{1} to {2}{3}", t.position.x, t.position.y, target.x, target.y));
        t.gameObject.GetComponent<GridItem>().isMoving = false;
    }

    public static IEnumerator Scale(this Transform t, Vector3 target, float duration)
    {
        Vector3 diffVecotor = (target - t.localScale);
        float diffLength = diffVecotor.magnitude;
        diffVecotor.Normalize();
        float counter = 0;
        while(counter < duration)
        {
            float movAmount = (Time.deltaTime * diffLength) / duration;
            t.localScale += diffVecotor * movAmount;
            counter += Time.deltaTime;
            yield return null;
        }

        t.localScale = target;
    }
}
