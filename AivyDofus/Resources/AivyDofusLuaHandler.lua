function protocol_required(callback, message, message_content)
	print('protocol required handled from lua')
	print(message.BasicString)
end

handlers:Add('ProtocolRequired', protocol_required)
