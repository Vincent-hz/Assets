using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

internal class AnimatedTexture : MonoBehaviour
{
    public int columns = 3;
    public float framesPerSecond = 10f;
    private int index;
    public int rows = 4;

    private void Start()
    {
        base.StartCoroutine(this.updateTiling());
        Vector2 scale = new Vector2(1f / ((float)this.columns), 1f / ((float)this.rows));
        base.GetComponent<Renderer>().sharedMaterial.SetTextureScale("_MainTex", scale);
    }

    [DebuggerHidden]
    private IEnumerator updateTiling()
    {
        return new updateTilingc__Iterator0 { f__this = this };
    }

    [CompilerGenerated]
    private sealed class updateTilingc__Iterator0 : IEnumerator<object>, IEnumerator, IDisposable
    {
        internal object current;
        internal int PC;
        internal AnimatedTexture f__this;
        internal Vector2 offset__0;

        [DebuggerHidden]
        public void Dispose()
        {
            this.PC = -1;
        }

        public bool MoveNext()
        {
            uint num = (uint)this.PC;
            this.PC = -1;
            switch (num)
            {
                case 0:
                    break;

                case 1:
                    break;
                    this.PC = -1;
                    goto Label_011D;

                default:
                    goto Label_011D;
            }
            this.f__this.index++;
            if (this.f__this.index >= (this.f__this.rows * this.f__this.columns))
            {
                this.f__this.index = 0;
            }
            this.offset__0 = new Vector2((((float)this.f__this.index) / ((float)this.f__this.columns)) - (this.f__this.index / this.f__this.columns), ((float)(this.f__this.index / this.f__this.columns)) / ((float)this.f__this.rows));
            this.f__this.GetComponent<Renderer>().sharedMaterial.SetTextureOffset("_MainTex", this.offset__0);
            this.current = new WaitForSeconds(1f / this.f__this.framesPerSecond);
            this.PC = 1;
            return true;
            Label_011D:
            return false;
        }

        [DebuggerHidden]
        public void Reset()
        {
            throw new NotSupportedException();
        }

        object IEnumerator<object>.Current
        {
            [DebuggerHidden]
            get
            {
                return this.current;
            }
        }

        object IEnumerator.Current
        {
            [DebuggerHidden]
            get
            {
                return this.current;
            }
        }
    }
}
