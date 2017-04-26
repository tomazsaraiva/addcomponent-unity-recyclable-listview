using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace AddComponent
{
	public class List : MonoBehaviour 
	{
		#region HANDLER ItemLoaded

		public delegate void OnItemLoadedHandler (ListItemBase item);

		public OnItemLoadedHandler onItemLoaded;

		public void ItemLoaded(ListItemBase item, bool clear = false)
		{
			if (onItemLoaded != null)
			{
				onItemLoaded(item);

				if (clear)
				{
					onItemLoaded = null;
				}
			}
		}

		#endregion

		#region HANDLER ItemSelected

		public delegate void OnItemSelectedHandler (ListItemBase item);

		public OnItemSelectedHandler onItemSelected;

		public void ItemSelected(ListItemBase item, bool clear = false)
		{
			if (onItemSelected != null)
			{
				onItemSelected(item);

				if (clear)
				{
					onItemSelected = null;
				}
			}
		}

		#endregion


		private enum Direction
		{
			DOWN,
			UP
		}

		[SerializeField]
		private ScrollRect _scrollRect;

		[SerializeField]
		private RectTransform _viewport;

		[SerializeField]
		private RectTransform _content;

		[SerializeField]
		private float _spacing;


		private List<ListItemBase> _items;

		private int _itemsTotal;
		private int _itemsVisible;

		private int _itemsToRecycleTop;
		private int _itemsToRecycleBottom;

		private float _listItemHeight;

		private float _lastPosition = -1;

		private int _currentIndex;


		public void Create(int items, ListItemBase listItemPrefab)
		{
			_listItemHeight = listItemPrefab.Size.y;

			_content.sizeDelta = new Vector2(_content.sizeDelta.x, _listItemHeight * items + _spacing * (items - 1));

			_itemsTotal = items;
			_itemsVisible = Mathf.FloorToInt (_viewport.rect.height / _listItemHeight) * 2;
			_itemsToRecycleBottom = _itemsVisible / 2;

			_currentIndex = _itemsVisible - 1;

			_items = new List<ListItemBase> ();

			for (int i = 0; i < _itemsVisible; i++)
			{
				ListItemBase item = CreateNewItem (listItemPrefab, i * (_listItemHeight + _spacing) + _listItemHeight / 2);
				item.onSelected = HandleOnSelectedHandler;
				item.Index = i;

				_items.Add(item);

				ItemLoaded (item);
			}

			_scrollRect.onValueChanged.AddListener ((Vector2 position) => 
			{
				Recycle();
			});	
		}

		public void Destroy()
		{
			_scrollRect.verticalNormalizedPosition = 1;

			if(_items != null)
			{
				for (int i = 0; i < _items.Count; i++)
				{
					Destroy (_items [i].gameObject);
				}

				_items.Clear ();
				_items = null;
			}

			_lastPosition = -1;
		}


		void HandleOnSelectedHandler (ListItemBase item)
		{
			ItemSelected (item);
		}


		private ListItemBase CreateNewItem(ListItemBase prefab, float positionY)
		{
			GameObject instance = (GameObject)Instantiate (prefab.gameObject, Vector3.zero, Quaternion.identity);
			instance.transform.SetParent (_content.transform);
			instance.transform.localScale = Vector3.one;
			instance.SetActive (true);

			RectTransform rectTransform = instance.GetComponent <RectTransform> ();
			rectTransform.anchoredPosition = new Vector2(0, -positionY);
			rectTransform.offsetMin = new Vector2 (0, rectTransform.offsetMin.y);
			rectTransform.offsetMax = new Vector2 (0, rectTransform.offsetMax.y);

			return instance.GetComponent <ListItemBase> ();
		}

		private void Recycle()
		{
			if(_lastPosition == -1)
			{
				_lastPosition = _content.anchoredPosition.y;

				return;
			}

			int rows = Mathf.FloorToInt (Mathf.Abs (_content.anchoredPosition.y - _lastPosition) / _listItemHeight);

			if (rows == 0)
			{
				return;
			}

			Direction direction = _lastPosition > _content.anchoredPosition.y ? Direction.UP : Direction.DOWN;

			for (int i = 0; i < rows; i++)
			{
				switch (direction)
				{
				case Direction.DOWN:

					if (_itemsToRecycleTop > _itemsVisible / 4 && _currentIndex < _itemsTotal - 1)
					{
						_currentIndex++;

						RecycleItem (direction);
					}
					else
					{
						_itemsToRecycleTop++; 
						_itemsToRecycleBottom--;
					}

					_lastPosition += _listItemHeight + _spacing;

					break;

				case Direction.UP:

					if (_itemsToRecycleBottom > _itemsVisible / 4 && _currentIndex > _itemsVisible - 1)
					{
						RecycleItem (direction);

						_currentIndex--;
					}
					else
					{
						_itemsToRecycleTop--; 
						_itemsToRecycleBottom++;
					}

					_lastPosition -= _listItemHeight + _spacing;

					break;
				}
			}
		}

		private void RecycleItem(Direction direction)
		{
			ListItemBase firstItem = _items [0];
			ListItemBase lastItem = _items [_items.Count - 1];

			float targetPositionY = (_listItemHeight + _spacing) * -1;

			switch(direction)
			{
			case Direction.DOWN:
				firstItem.Position = new Vector2 (firstItem.Position.x, lastItem.Position.y + targetPositionY);
				firstItem.Index = _currentIndex;
				firstItem.transform.SetAsLastSibling ();

				_items.RemoveAt (0);
				_items.Add (firstItem);

				ItemLoaded (firstItem);
				break;

			case Direction.UP:
				lastItem.Position = new Vector2 (lastItem.Position.x, firstItem.Position.y - targetPositionY);
				lastItem.Index = _currentIndex - _itemsVisible;
				lastItem.transform.SetAsFirstSibling ();

				_items.RemoveAt (_items.Count - 1);
				_items.Insert (0, lastItem);

				ItemLoaded (lastItem);
				break;
			}
		}
	}	
}