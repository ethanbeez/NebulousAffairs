using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class NebulaMath {
    public static Vector3 RotateAroundBody(Vector3 rotatingBodyPos, Vector3 otherBodyPos, Vector3 axis, float degree) {
        Quaternion quat = Quaternion.AngleAxis(degree, axis);
        return quat * (rotatingBodyPos - otherBodyPos) + otherBodyPos;
    }
}
