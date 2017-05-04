//
//  ListItemMovie.cs
//
//  Author:
//       tomazsaraiva <>
//
//  Copyright (c) 2017 tomazsaraiva
using UnityEngine;
using System.Collections;
using AddComponent;
using UnityEngine.UI;

public class ListItemMovie : ListItemBase
{
	[SerializeField]
	private Image _image;

	[SerializeField]
	private AspectRatioFitter _aspectRatioFitter;


	private Sprite _sprite;


	public void SetImage(Sprite sprite)
	{
		if(_sprite != null)
		{
			Resources.UnloadAsset (_sprite.texture);
		}

		_sprite = sprite;

		_aspectRatioFitter.aspectRatio = (float)sprite.texture.width / (float)sprite.texture.height;

		_image.sprite = sprite;
	}
}