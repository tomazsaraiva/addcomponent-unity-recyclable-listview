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


		private enum ScrollOrientation
		{
			HORIZONTAL,
			VERTICAL
		}

		private enum ScrollDirection
		{
			NEXT,
			PREVIOUS
		}


		[SerializeField]
		private ScrollRect _scrollRect;

		[SerializeField]
		private RectTransform _viewport;

		[SerializeField]
		private RectTransform _content;

		[SerializeField]
		private ScrollOrientation _scrollOrientation;

		[SerializeField]
		private float _spacing;


		private List<ListItemBase> _itemsList;

		private int _itemsTotal;

		private int _itemsToRecycleBefore;
		private int _itemsToRecycleAfter;

		private Vector2 _listItemSize;

		private float _lastPosition = -1;

		private int _currentIndex;


		public void Create(int items, ListItemBase listItemPrefab)
		{
			switch (_scrollOrientation)
			{
			case ScrollOrientation.HORIZONTAL:
				_scrollRect.vertical = false;
				_scrollRect.horizontal = true;

				_content.anchorMin = new Vector2 (0, 0);
				_content.anchorMax = new Vector2 (0, 1);
				break;

			case ScrollOrientation.VERTICAL:
				_scrollRect.vertical = true;
				_scrollRect.horizontal = false;

				_content.anchorMin = new Vector2 (0, 1);
				_content.anchorMax = new Vector2 (1, 1);
				break;
			}

			_content.sizeDelta = GetContentSize(items, listItemPrefab);

			_itemsList = CreateItemsList (listItemPrefab);


			_listItemSize = listItemPrefab.Size;
			_itemsTotal = items;

			_currentIndex = _itemsList.Count - 1;

			_itemsToRecycleAfter = _itemsList.Count / 2;


			_scrollRect.onValueChanged.AddListener ((Vector2 position) => 
			{
				Recycle();
			});	
		}

		public void Destroy()
		{
			_scrollRect.verticalNormalizedPosition = 1;

			if(_itemsList != null)
			{
				for (int i = 0; i < _itemsList.Count; i++)
				{
					Destroy (_itemsList [i].gameObject);
				}

				_itemsList.Clear ();
				_itemsList = null;
			}

			_lastPosition = -1;
		}


		void HandleOnSelectedHandler (ListItemBase item)
		{
			ItemSelected (item);
		}


		private Vector2 GetContentSize(int items, ListItemBase prefab)
		{
			Vector2 contentSize = Vector2.zero;
			float contentSizeMultiplier = items + _spacing * (items - 1);

			switch (_scrollOrientation)
			{
			case ScrollOrientation.HORIZONTAL:
				contentSize.x = prefab.Size.x * contentSizeMultiplier;
				break;

			case ScrollOrientation.VERTICAL:
				contentSize.y = prefab.Size.y * contentSizeMultiplier;
				break;
			}

			return contentSize;
		}

		private List<ListItemBase> CreateItemsList(ListItemBase prefab)
		{
			List<ListItemBase> itemsList = new List<ListItemBase> ();

			float itemDimension = 0;
			float viewportDimension = 0;

			switch (_scrollOrientation)
			{
			case ScrollOrientation.HORIZONTAL:
				viewportDimension = _viewport.rect.width;
				itemDimension = prefab.Size.x;
				break;

			case ScrollOrientation.VERTICAL:
				viewportDimension = _viewport.rect.height;
				itemDimension = prefab.Size.y;
				break;
			}

			int itemsToInstantiate = Mathf.FloorToInt (viewportDimension / itemDimension) * 2;

			for (int i = 0; i < itemsToInstantiate; i++)
			{
				ListItemBase item = CreateNewItem (prefab, i, itemDimension);
				item.onSelected = HandleOnSelectedHandler;
				item.Index = i;

				itemsList.Add(item);

				ItemLoaded (item);
			}

			return itemsList;
		}


		private ListItemBase CreateNewItem(ListItemBase prefab, int index, float dimension)
		{
			GameObject instance = (GameObject)Instantiate (prefab.gameObject, Vector3.zero, Quaternion.identity);
			instance.transform.SetParent (_content.transform);
			instance.transform.localScale = Vector3.one;
			instance.SetActive (true);

			float position = index * (dimension + _spacing) + dimension / 2;

			RectTransform rectTransform = instance.GetComponent <RectTransform> ();

			switch (_scrollOrientation)
			{
			case ScrollOrientation.HORIZONTAL:
				rectTransform.anchorMin = new Vector2 (0, 0);
				rectTransform.anchorMax = new Vector2 (0, 1);
				rectTransform.anchoredPosition = new Vector2 (position, 0);
				rectTransform.offsetMin = new Vector2 (rectTransform.offsetMin.x, 0);
				rectTransform.offsetMax = new Vector2 (rectTransform.offsetMax.x, 0);
				break;

			case ScrollOrientation.VERTICAL:
				rectTransform.anchorMin = new Vector2 (0, 1);
				rectTransform.anchorMax = new Vector2 (1, 1);
				rectTransform.anchoredPosition = new Vector2 (0, -position);
				rectTransform.offsetMin = new Vector2 (0, rectTransform.offsetMin.y);
				rectTransform.offsetMax = new Vector2 (0, rectTransform.offsetMax.y);
				break;
			}

			return instance.GetComponent <ListItemBase> ();
		}

		private void Recycle()
		{
			if(_lastPosition == -1)
			{
				_lastPosition = _content.anchoredPosition.y;

				return;
			}

			int rows = Mathf.FloorToInt (Mathf.Abs (_content.anchoredPosition.y - _lastPosition) / _listItemSize.y);

			if (rows == 0)
			{
				return;
			}

			ScrollDirection direction = _lastPosition > _content.anchoredPosition.y ? ScrollDirection.PREVIOUS : ScrollDirection.NEXT;

			for (int i = 0; i < rows; i++)
			{
				switch (direction)
				{
				case ScrollDirection.NEXT:

					if (_itemsToRecycleBefore > _itemsList.Count / 4 && _currentIndex < _itemsTotal - 1)
					{
						_currentIndex++;

						RecycleItem (direction);
					}
					else
					{
						_itemsToRecycleBefore++; 
						_itemsToRecycleAfter--;
					}

					_lastPosition += _listItemSize.y + _spacing;

					break;

				case ScrollDirection.PREVIOUS:

					if (_itemsToRecycleAfter > _itemsList.Count / 4 && _currentIndex > _itemsList.Count - 1)
					{
						RecycleItem (direction);

						_currentIndex--;
					}
					else
					{
						_itemsToRecycleBefore--; 
						_itemsToRecycleAfter++;
					}

					_lastPosition -= _listItemSize.y + _spacing;

					break;
				}
			}
		}

		private void RecycleItem(ScrollDirection direction)
		{
			ListItemBase firstItem = _itemsList [0];
			ListItemBase lastItem = _itemsList [_itemsList.Count - 1];

			float targetPositionY = (_listItemSize.y + _spacing) * -1;

			switch(direction)
			{
			case ScrollDirection.NEXT:
				firstItem.Position = new Vector2 (firstItem.Position.x, lastItem.Position.y + targetPositionY);
				firstItem.Index = _currentIndex;
				firstItem.transform.SetAsLastSibling ();

				_itemsList.RemoveAt (0);
				_itemsList.Add (firstItem);

				ItemLoaded (firstItem);
				break;

			case ScrollDirection.PREVIOUS:
				lastItem.Position = new Vector2 (lastItem.Position.x, firstItem.Position.y - targetPositionY);
				lastItem.Index = _currentIndex - _itemsList.Count;
				lastItem.transform.SetAsFirstSibling ();

				_itemsList.RemoveAt (_itemsList.Count - 1);
				_itemsList.Insert (0, lastItem);

				ItemLoaded (lastItem);
				break;
			}
		}
	}	
}