using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMovePlayer
{
    void Move(Vector3 direction);
    void Dash(Vector3 direction);
}
