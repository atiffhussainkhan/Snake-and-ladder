using System;
using System.Collections;
using UnityEngine;

namespace SnakeLadder.Prototype
{
    // Lives on the Dice GameObject. Spins the die for a configurable duration so the player
    // gets a beat of suspense before the final value appears, then settles on the chosen face
    // so the lucky number is readable from the camera.
    public class DiceRoller : MonoBehaviour
    {
        public float rollDuration = 3.0f;
        public float tickInterval = 0.06f;          // ~16 visible face changes per second

        // Final rotation that brings each face number upward so the camera (looking down from above-front) reads it.
        private static readonly Quaternion[] FaceUpRotation =
        {
            Quaternion.identity,                  // [0] unused
            Quaternion.identity,                  // 1 — top face normal is already +Y
            Quaternion.Euler(-90f, 0f, 0f),       // 2 — front face (+Z) rotates up to +Y
            Quaternion.Euler(0f, 0f, 90f),        // 3 — right face (+X) rotates up to +Y
            Quaternion.Euler(0f, 0f, -90f),       // 4 — left face (-X) rotates up to +Y
            Quaternion.Euler(90f, 0f, 0f),        // 5 — back face (-Z) rotates up to +Y
            Quaternion.Euler(180f, 0f, 0f),       // 6 — bottom face (-Y) flips to +Y
        };

        private bool _rolling;

        public bool IsRolling => _rolling;

        // Called by DiceController.Roll. Spins for `rollDuration` seconds, then lands on the
        // chosen face and fires onComplete with the final roll value.
        public void StartRoll(int finalNumber, Action<int> onComplete)
        {
            if (_rolling) return; // ignore re-entry while a roll is in flight
            _rolling = true;
            StartCoroutine(RollCoroutine(finalNumber, onComplete));
        }

        private IEnumerator RollCoroutine(int finalNumber, Action<int> onComplete)
        {
            float elapsed = 0f;

            while (elapsed < rollDuration)
            {
                transform.rotation = UnityEngine.Random.rotationUniform;
                yield return new WaitForSeconds(tickInterval);
                elapsed += tickInterval;
            }

            // Land cleanly on the chosen face — small bonus spin during the snap for visual polish.
            Quaternion targetRotation = FaceUpRotation[finalNumber];
            float settleDuration = 0.25f;
            float settleElapsed = 0f;
            Quaternion startRotation = transform.rotation;
            while (settleElapsed < settleDuration)
            {
                settleElapsed += Time.deltaTime;
                float t = Mathf.Clamp01(settleElapsed / settleDuration);
                // Ease-out cubic so the die decelerates into the final face.
                float eased = 1f - Mathf.Pow(1f - t, 3f);
                transform.rotation = Quaternion.Slerp(startRotation, targetRotation, eased);
                yield return null;
            }
            transform.rotation = targetRotation;

            _rolling = false;
            onComplete?.Invoke(finalNumber);
        }
    }
}
