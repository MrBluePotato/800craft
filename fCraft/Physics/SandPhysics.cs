﻿/*        ----
        Copyright (c) 2011-2013 Jon Baker, Glenn Marien and Lao Tszy <Jonty800@gmail.com>
        All rights reserved.

        Redistribution and use in source and binary forms, with or without
        modification, are permitted provided that the following conditions are met:
         * Redistributions of source code must retain the above copyright
              notice, this list of conditions and the following disclaimer.
            * Redistributions in binary form must reproduce the above copyright
             notice, this list of conditions and the following disclaimer in the
             documentation and/or other materials provided with the distribution.
            * Neither the name of 800Craft or the names of its
             contributors may be used to endorse or promote products derived from this
             software without specific prior written permission.

        THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
        ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
        WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
        DISCLAIMED. IN NO EVENT SHALL <COPYRIGHT HOLDER> BE LIABLE FOR ANY
        DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
        (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
        LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
        ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
        (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
        SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
        ----*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using fCraft.Events;

namespace fCraft
{
    public class SandTask : PhysicsTask
    {
        private const int Delay = 200;
        private Vector3I _pos;
        private int _nextPos;
        private bool _firstMove = true;
        private Block _type;
        public SandTask(World world, Vector3I position, Block Type)
            : base(world)
        {
            _pos = position;
            _nextPos = position.Z - 1;
            _type = Type;
        }

        protected override int PerformInternal()
        {
            lock (_world.SyncRoot)
            {
                if (_world.sandPhysics)
                {
                    Block nblock = _world.Map.GetBlock(_pos.X, _pos.Y, _nextPos);
                    if (_firstMove)
                    {
                        if (_world.Map.GetBlock(_pos) != _type)
                        {
                            return 0;
                        }
                        if (_world.Map.GetBlock(_pos.X, _pos.Y, _nextPos) == Block.Air)
                        {
                            _world.Map.QueueUpdate(new BlockUpdate(null, _pos, Block.Air));
                            _world.Map.QueueUpdate(new BlockUpdate(null, (short)_pos.X, (short)_pos.Y, (short)_nextPos, _type));
                            _nextPos--;
                            _firstMove = false;
                            return Delay;
                        }
                    }
                    if (_world.Map.GetBlock(_pos.X, _pos.Y, _nextPos) != Block.Air)
                    {
                        return 0;
                    }
                    if (Physics.BlockThrough(nblock))
                    {
                        _world.Map.QueueUpdate(new BlockUpdate(null, (short)_pos.X, (short)_pos.Y, (short)(_nextPos + 1), Block.Air));
                        _world.Map.QueueUpdate(new BlockUpdate(null, (short)_pos.X, (short)_pos.Y, (short)_nextPos, _type));
                        _nextPos--;
                    }
                }
                return Delay;
            }
        }
    }
}
