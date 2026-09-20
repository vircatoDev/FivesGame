using DG.Tweening;
using TMPro;
using UnityEngine;

namespace Scripts.UI
{
    public class CurrencyAnimationComponent : MonoBehaviour
    {
        public TextMeshProUGUI AmountText;


        public void PlayAnimation(int amount)
        {
            AmountText.text = amount.ToString();
            AmountText.color = amount > 0 ? Color.green : Color.red;
            AmountText.gameObject.SetActive(true);
            AmountText.transform.DOMoveY(AmountText.transform.position.y - 1f, 1f);
            AmountText.DOFade(0, 1f).OnComplete(() => { Destroy(gameObject); });
        }
    }
}