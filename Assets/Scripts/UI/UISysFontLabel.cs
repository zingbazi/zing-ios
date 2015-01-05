// Additions Copyright (c) 2013 Empirical Development LLC. All rights reserved.

/*
 * Copyright (c) 2012 Mario Freitas (imkira@gmail.com)
 * 
 * Permission is hereby granted, free of charge, to any person obtaining
 * a copy of this software and associated documentation files (the
 * "Software"), to deal in the Software without restriction, including
 * without limitation the rights to use, copy, modify, merge, publish,
 * distribute, sublicense, and/or sell copies of the Software, and to
 * permit persons to whom the Software is furnished to do so, subject to
 * the following conditions:
 * 
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 * EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
 * MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
 * LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
 * OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
 * WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using UnityEngine;
using System.Text;

[ExecuteInEditMode]
[AddComponentMenu("NGUI/UI/SysFont Label")]
public class UISysFontLabel : UIWidget, ISysFontTexturable
{
	[SerializeField]
	protected SysFontTexture _texture = new SysFontTexture();
	
//	private bool needsUpdate = true;
	
	[SerializeField]
	private float mLocalClipWidth;
	public float LocalClipWidth
	{
		get { return mLocalClipWidth; }
		set
		{
			if(mLocalClipWidth != value) {
				mLocalClipWidth = value;
				MarkAsChanged();
			}
		}
	}
	
	[SerializeField]
	private bool mClipToBounds;
	public bool ClipToBounds
	{
		get { return mClipToBounds; }
		set
		{
			if(mClipToBounds != value) {
				mClipToBounds = value;
				if(LocalClipWidth == 0) {
					LocalClipWidth = cachedTransform.localScale.x;
				}
				MarkAsChanged();
			}
		}
	}
	
	[SerializeField]
	private bool mRightToLeftText;
	public bool RightToLeftText
	{
		get { return mRightToLeftText; }
		set
		{
			if(mRightToLeftText != value) {
				mRightToLeftText = value;
				MarkAsChanged();
			}
		}
	}
	
  public SysFontTexture FontTexture
  {
	get
	{
	  return _texture;		
	}
  }

  #region ISysFontTexturable properties
	
	[SerializeField]
	private string mText;

	public string Text
	{
		get
		{
			return mText;
		}
		set
		{
			if(mText == null || !mText.Equals(value)) {
				mText = value;
				
				if(password) {
					StringBuilder sb = new StringBuilder();
					for(int i = 0; i < mText.Length; i++) {
						sb.Append("*");
					}
					int last = mText.Length - 1;
					if(showLastPasswordChar && last >= 0) {
						sb[last] = mText[last];
					}
					_texture.Text = sb.ToString();
				} else {
					_texture.Text = mText;
				}
			}
			
		}
	}

  public string AppleFontName
  {
    get
    {
      return _texture.AppleFontName;
    }
    set
    {
      _texture.AppleFontName = value;
    }
  }

  public string AndroidFontName
  {
    get
    {
      return _texture.AndroidFontName;
    }
    set
    {
      _texture.AndroidFontName = value;
    }
  }

  public string FontName
  {
    get
    {
      return _texture.FontName;
    }
    set
    {
      _texture.FontName = value;
    }
  }

  public int FontSize
  {
    get
    {
      return _texture.FontSize;
    }
    set
    {
      _texture.FontSize = value;
    }
  }

  public bool IsBold
  {
    get
    {
      return _texture.IsBold;
    }
    set
    {
      _texture.IsBold = value;
    }
  }

  public bool IsItalic
  {
    get
    {
      return _texture.IsItalic;
    }
    set
    {
      _texture.IsItalic = value;
    }
  }

  public SysFont.Alignment Alignment
  {
    get
    {
      return _texture.Alignment;
    }
    set
    {
      _texture.Alignment = value;
    }
  }

  public bool IsMultiLine
  {
    get
    {
      return _texture.IsMultiLine;
    }
    set
    {
      _texture.IsMultiLine = value;
    }
  }

  public int MaxWidthPixels
  {
    get
    {
      return _texture.MaxWidthPixels;
    }
    set
    {
      _texture.MaxWidthPixels = value;
    }
  }

  public int MaxHeightPixels
  {
    get
    {
      return _texture.MaxHeightPixels;
    }
    set
    {
      _texture.MaxHeightPixels = value;
    }
  }

  public int WidthPixels 
  {
    get
    {
      return _texture.WidthPixels;
    }
  }

  public int HeightPixels 
  {
    get
    {
      return _texture.HeightPixels;
    }
  }

  public int TextWidthPixels 
  {
    get
    {
      return _texture.TextWidthPixels;
    }
  }

  public int TextHeightPixels 
  {
    get
    {
      return _texture.TextHeightPixels;
    }
  }

  public Texture Texture
  {
    get
    {
      return _texture.Texture;
    }
  }
  #endregion
  static protected Shader _shader = null;
  protected Material _createdMaterial = null;
  protected Vector3[] _vertices = null;
  protected Vector2 _uv;
	
	#region Events and Delegates
	
	public delegate void FontLabelTextureChangedEvent(UISysFontLabel label);
	public event FontLabelTextureChangedEvent FontLabelTextureDidChange;
	
	#endregion
	
	
  #region UIWidget
	public override bool keepMaterial
  {
    get
    {
      return true;
    }
  }
	public int textHeightAdjust = 6;

	public override void Update()
	{
		base.Update();
		UpdateTextureIfNecessary();
	}

	public void UpdateTextureIfNecessary()
	{
		if(_texture.NeedsRedraw)
		{
			_texture.Update();
			_uv = new Vector2(_texture.TextWidthPixels / (float)_texture.WidthPixels, (_texture.TextHeightPixels + textHeightAdjust) / (float)_texture.HeightPixels);

			if(FontLabelTextureDidChange != null) {
				FontLabelTextureDidChange(this);
			}
			MarkAsChanged();
		}
	}

  override public void MakePixelPerfect()
  {
    Vector3 scale = cachedTransform.localScale;
    scale.x = _texture.TextWidthPixels;
    scale.y = _texture.TextHeightPixels + textHeightAdjust;
    cachedTransform.localScale = scale;

    base.MakePixelPerfect();
  }
	
	override public void OnFill(BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color32> cols)
	{
	    if (_vertices == null)
	    {
			_vertices = new Vector3[4];
			_vertices[0] = new Vector3(1f,  0f, 0f);
			_vertices[1] = new Vector3(1f, -1f, 0f);
			_vertices[2] = new Vector3(0f, -1f, 0f);
			_vertices[3] = new Vector3(0f,  0f, 0f);
	    }

		verts.Add(_vertices[0]);
		verts.Add(_vertices[1]);
		verts.Add(_vertices[2]);
		verts.Add(_vertices[3]);
		
		float scrolledX = RightToLeftText ?  _uv.x : 0f;
		float textWidthPixels = (float)_texture.TextWidthPixels;
		if(ClipToBounds && LocalClipWidth <= textWidthPixels) {
			
			float uvWidth = (LocalClipWidth / textWidthPixels) * _uv.x;
			if(RightToLeftText) {
				scrolledX = uvWidth;
			} else {
				scrolledX = _uv.x - uvWidth;
			}
			

			float localWidth = (float)_texture.TextWidthPixels;
			Vector3 scale = cachedTransform.localScale;
			scale.x = Mathf.Clamp(localWidth, 0f, LocalClipWidth);
			scale.y = _texture.TextHeightPixels + textHeightAdjust;
			cachedTransform.localScale = scale;
		} else {
			MakePixelPerfect();
		}
		
		if(RightToLeftText) {
		    uvs.Add(new Vector2(scrolledX, _uv.y));
		    uvs.Add(new Vector2(scrolledX, 0f));
		    uvs.Add(Vector2.zero);
		    uvs.Add(new Vector2(0f, _uv.y));
		} else {
			uvs.Add(_uv);
			uvs.Add(new Vector2(_uv.x, 0f));
			uvs.Add(new Vector2(scrolledX, 0f));
			uvs.Add(new Vector2(scrolledX, _uv.y));
		}
		
		Color col = color;
		col.a *= mPanel.alpha;
		
		cols.Add(col);
		cols.Add(col);
		cols.Add(col);
		cols.Add(col);
		
		if (material.mainTexture != _texture.Texture)
		{
			material.mainTexture = _texture.Texture;
		}
//		mChanged = false;
	}
  #endregion
	
	#region UILabel stuff
	
	public bool showLastPasswordChar = true;
	public bool password = false;
	public bool supportEncoding = false;
	
	#endregion

  #region MonoBehaviour methods
  protected override void OnEnable()
  {
    if (_shader == null)
    {
      _shader = Shader.Find("Unlit/Transparent Colored (Packed)");
    }

    if (_createdMaterial == null)
    {
      _createdMaterial = new Material(_shader);
      _createdMaterial.hideFlags =
        HideFlags.HideInInspector | HideFlags.DontSave;
      _createdMaterial.mainTexture = _texture.Texture;
      material = _createdMaterial;
    }
  }

  protected void OnDestroy()
  {
    material = null;
    SysFont.SafeDestroy(_createdMaterial);
    if (_texture != null)
    {
      _texture.Destroy();
      _texture = null;
    }
	}
  #endregion
}
