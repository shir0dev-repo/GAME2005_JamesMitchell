using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnimplementedCollisionException : System.Exception
{
    public UnimplementedCollisionException() : base() { }
    public UnimplementedCollisionException(string message) : base(message) { }
}
