﻿using System.Collections;
using UnityEngine;

namespace CryptoQuest.Battle.Commands
{
    public class SkipTurnCommand : ICommand
    {
        private readonly Components.Character _character;

        public SkipTurnCommand(Components.Character character)
        {
            _character = character;
        }

        public void Execute()
        {
            Debug.Log($"{_character.DisplayName} Skip turn");
        }
    }
}