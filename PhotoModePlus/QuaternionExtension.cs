using UnityEngine;

public static class QuaternionExtension {
    public static Quaternion WithEulerAngles(this Quaternion quaternion, float? x = null, float? y = null, float? z = null) {
        Vector3 eulerAngles = quaternion.eulerAngles;
        eulerAngles.Set(x ?? eulerAngles.x, y ?? eulerAngles.y, z ?? eulerAngles.z);
        return Quaternion.Euler(eulerAngles);
    }
}