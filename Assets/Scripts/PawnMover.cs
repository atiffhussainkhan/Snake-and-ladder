using System;
using System.Collections;
using UnityEngine;

namespace SnakeLadder.Prototype
{
    // Lives on the bootstrap GameObject. Animates pawn movement one square at a time so the
    // player can follow each step, plus a smooth lerp for snake/ladder jumps.
    public class PawnMover : MonoBehaviour
    {
        public float stepDuration   = 0.22f;   // time to walk from one square to the next
        public float stepPause      = 0.06f;   // tiny pause at each square for readability
        public float jumpDuration   = 0.55f;   // snake/ladder jump duration

        // Walks pawn `pawnIndex` from `fromSquare` to `toSquare` one square at a time,
        // bumping the static PawnSpawner.{Pawn}Square counter as it lands on each square.
        public IEnumerator MoveStepByStep(int pawnIndex, int fromSquare, int toSquare)
        {
            int pawnOffsetX = pawnIndex == 1 ? -1 : 1;
            int pawnOffsetZ = pawnIndex == 1 ?  1 : -1;

            if (fromSquare == toSquare) yield break;
            int step = fromSquare < toSquare ? 1 : -1;

            for (int sq = fromSquare + step; ; sq += step)
            {
                var pawn = GetPawn(pawnIndex);
                if (pawn == null) yield break;
                Vector3 worldPos = BoardBuilder.PositionForSquare(sq) + new Vector3(0.28f * pawnOffsetX, 0f, -0.28f * pawnOffsetZ);
                yield return StartCoroutine(MoveTransform(pawn.transform, pawn.transform.position, worldPos, stepDuration));
                if (pawn == null) yield break;

                // Track the static counter as we land so the StressSimulator / WinBanner see the up-to-date value.
                if (pawnIndex == 1) PawnSpawner.Pawn1Square = sq;
                else                PawnSpawner.Pawn2Square = sq;

                if (sq == toSquare) break;
                if (stepPause > 0f) yield return new WaitForSeconds(stepPause);
            }
        }

        // Smooth lerp from current position to the destination square — used for snake/ladder jumps.
        public IEnumerator JumpTo(int pawnIndex, int targetSquare)
        {
            int pawnOffsetX = pawnIndex == 1 ? -1 : 1;
            int pawnOffsetZ = pawnIndex == 1 ?  1 : -1;
            Vector3 worldPos = BoardBuilder.PositionForSquare(targetSquare) + new Vector3(0.28f * pawnOffsetX, 0f, -0.28f * pawnOffsetZ);
            var pawn = GetPawn(pawnIndex);
            if (pawn == null) yield break;
            yield return StartCoroutine(MoveTransform(pawn.transform, pawn.transform.position, worldPos, jumpDuration));
            if (pawn == null) yield break;

            if (pawnIndex == 1) PawnSpawner.Pawn1Square = targetSquare;
            else                PawnSpawner.Pawn2Square = targetSquare;
        }

        private static GameObject GetPawn(int pawnIndex)
        {
            return pawnIndex == 1 ? PawnSpawner.Pawn1 : PawnSpawner.Pawn2;
        }

        // Smoothly moves `target` from `from` to `to` over `duration` seconds with an arc
        // (parabolic Y bump) so the jump looks like an actual hop rather than a slide.
        private static IEnumerator MoveTransform(Transform target, Vector3 from, Vector3 to, float duration)
        {
            if (target == null || duration <= 0f)
            {
                if (target != null) target.position = to;
                yield break;
            }
            float elapsed = 0f;
            float arc = 0.18f;
            while (elapsed < duration)
            {
                if (target == null) yield break;
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                Vector3 pos = Vector3.Lerp(from, to, t);
                // Parabolic arc — only adds lift during the middle of the move.
                pos.y += 4f * t * (1f - t) * arc;
                target.position = pos;
                yield return null;
            }
            if (target == null) yield break;
            target.position = to;
        }
    }
}
