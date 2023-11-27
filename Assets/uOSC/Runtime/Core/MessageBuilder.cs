using System;
using System.Collections.Generic;
using UnityEngine;

namespace uOSC
{
    public class MessageBuilder
    {
        private string _address;
        private readonly List<OCSValue> _buffer = new();

        public MessageBuilder Address(string address)
        {
            _address = address;
            return this;
        }
        
        public MessageBuilder AddFloat(float value)
        {
            AddValue(value);
            return this;
        }
        
        public MessageBuilder AddInt(int value)
        {
            AddValue(value);
            return this;
        }
        
        public MessageBuilder AddString(string value)
        {
            AddValue(value);
            return this;
        }

        public MessageBuilder AddVector2(Vector2 value)
        {
            AddValue(value.x);
            AddValue(value.y);
            return this;
        }

        public MessageBuilder AddVector3(Vector3 value)
        {
            AddValue(value.x);
            AddValue(value.y);
            AddValue(value.z);
            return this;
        }
        
        public MessageBuilder AddQuaternion(Quaternion value)
        {
            AddValue(value.x);
            AddValue(value.y);
            AddValue(value.z);
            AddValue(value.w);
            return this;
        }
        
        public MessageBuilder AddBlob(byte[] data, int offset, int count)
        {
            return AddBlob(data.AsSpan(offset, count));
        }
        
        public MessageBuilder AddBlob(ReadOnlySpan<byte> data)
        {
            _buffer.Add(data);
            return this;
        }

        public void Reset()
        {
            _address = string.Empty;
            _buffer.Clear();
        }

        public Message Build()
        {
            if (string.IsNullOrEmpty(_address))
            {
                throw new InvalidOperationException("Message address is null or empty, did you forget to call builder.Address()");
            }
            
            var msg = new Message(_address, _buffer);
            Reset();
            
            return msg;
        }
        
        private MessageBuilder AddValue(OCSValue value)
        {
            _buffer.Add(value);
            return this;
        }
    }
}