﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using KSP;
using Strategies;
using Strategies.Effects;

namespace Strategia
{
    /// <summary>
    /// Special CurrencyOperation only for facility upgrades
    /// </summary>
    public class CurrencyOperationFacilityUpgrade : CurrencyOperation
    {
        float fundsDelta;
        float reputationDelta;
        float scienceDelta;

        public CurrencyOperationFacilityUpgrade(Strategy parent)
            : base(parent)
        {
        }

        protected override void OnLoadFromConfig(ConfigNode node)
        {
            node.AddValue("effectDescription", "on facility upgrades");
            node.AddValue("AffectReasons", "StructureConstruction");

            base.OnLoadFromConfig(node);
        }

        protected override void OnRegister()
        {
            GameEvents.Modifiers.OnCurrencyModifierQuery.Add(new EventData<CurrencyModifierQuery>.OnEvent(OnEffectQuery));
            GameEvents.Modifiers.OnCurrencyModified.Add(new EventData<CurrencyModifierQuery>.OnEvent(OnCurrencyModified));
        }

        protected override void OnUnregister()
        {
            GameEvents.Modifiers.OnCurrencyModifierQuery.Remove(new EventData<CurrencyModifierQuery>.OnEvent(OnEffectQuery));
            GameEvents.Modifiers.OnCurrencyModified.Remove(new EventData<CurrencyModifierQuery>.OnEvent(OnCurrencyModified));
        }

        private void OnEffectQuery(CurrencyModifierQuery qry)
        {
            if (qry.reason != TransactionReasons.StructureConstruction)
            {
                return;
            }

            fundsDelta = qry.GetEffectDelta(Currency.Funds);
            reputationDelta = qry.GetEffectDelta(Currency.Reputation);
            scienceDelta = qry.GetEffectDelta(Currency.Science);

            MethodInfo oeqMethod = typeof(CurrencyOperation).GetMethod("OnEffectQuery", BindingFlags.Instance | BindingFlags.NonPublic);
            oeqMethod.Invoke(this, new object[] { qry });

            // Calculate any changes
            fundsDelta = qry.GetEffectDelta(Currency.Funds) - fundsDelta;
            reputationDelta = qry.GetEffectDelta(Currency.Reputation) - reputationDelta;
            scienceDelta = qry.GetEffectDelta(Currency.Science) - scienceDelta;
        }

        void OnCurrencyModified(CurrencyModifierQuery qry)
        {
            Debug.Log("OnCurrencyModified");
            if (qry.reason == TransactionReasons.StructureConstruction)
            {
                // Check for changes
                if (Math.Abs(fundsDelta) > 0.01)
                {
                    CurrencyPopup.Instance.AddFacilityPopup(Currency.Funds, fundsDelta, Parent.Config.Title, true);
                }
                if (Math.Abs(reputationDelta) > 0.01)
                {
                    CurrencyPopup.Instance.AddFacilityPopup(Currency.Reputation, reputationDelta, Parent.Config.Title, true);
                }
                if (Math.Abs(scienceDelta) > 0.01)
                {
                    CurrencyPopup.Instance.AddFacilityPopup(Currency.Science, scienceDelta, Parent.Config.Title, true);
                }
            }
        }
    }
}
