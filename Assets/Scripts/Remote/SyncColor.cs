using HoloToolkit.Sharing.SyncModel;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SyncDataClass]
public class SyncColor : SyncObject {

    [SyncData] private SyncFloat r = null;
    [SyncData] private SyncFloat g = null;
    [SyncData] private SyncFloat b = null;
    [SyncData] private SyncFloat a = null;

#if UNITY_EDITOR
    public override object RawValue
    {
        get { return Value; }
    }
#endif

    public Color Value
    {
        get { return new Color(r.Value, g.Value, b.Value, a.Value); }
        set
        {
            r.Value = value.r;
            g.Value = value.g;
            b.Value = value.b;
            a.Value = value.a;
        }
    }

    public SyncColor(string field) : base(field) { }
}
