-- HANDLER FORM --

-- AbstractClientReceiveCallback * NetworkElement * NetworkContentElement -> bool
-- function your_handler_function(AbstractClientReceiveCallback, NetworkElement, NetworkContentElement) 				-- creating a handler function
--		return true 																									-- return true if message handled will be forwarded else false (if false you'll have to send the msg manualy)
-- end
-- handlers:Add('YouMessageHandlerName', your_handler_function) 														-- adding handler to handlers list

-- END HANDLER FORM --

-- AbstractClientReceiveCallback * NetworkElement * NetworkContentElement -> bool
-- same as c# handler , check README and c# source code for more information
function protocol_required(callback, message, message_content)
	print('from lua : '..message.BasicString)
	print(message_content)
	return true
end

function hello_connect_message(callback, message, message_content)
	print('from lua : '..message.BasicString)
	print(message_content)
	return true
end

handlers:Add('ProtocolRequired', protocol_required)
handlers:Add('HelloConnectMessage', hello_connect_message)