using UnityEngine;
using Utilities;

namespace RpgPractice
{
    public interface IDetectionStrategy
    {
        bool Execute(Transform player, Transform detector, CountdownTimer timer);
    }
}