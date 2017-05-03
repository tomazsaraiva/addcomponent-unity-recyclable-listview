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


	public void SetImage(Sprite sprite)
	{
		_image.sprite = sprite;
	}
}