
function servers_list_message(callback, message, message_content) -- from server
	local new_servers_list = {}
	
	local i = 0
	while i < message_content['servers'].Length do
		new_servers_list[i] = message_content['servers'][i]
		i = i + 1
	end
	
	new_servers_list[i] = NetworkContentElement()
	new_servers_list[i]['isMonoAccount'] = true
	new_servers_list[i]['isSelectable'] = true
	new_servers_list[i]['id'] = 127
	new_servers_list[i]['type'] = 1
	new_servers_list[i]['status'] = 3
	new_servers_list[i]['completion'] = 0
	new_servers_list[i]['charactersCount'] = 1
	new_servers_list[i]['charactersSlots'] = 1
	new_servers_list[i]['date'] = 1234828800000
	
	message_content['servers'] = new_servers_list
	
	send_message(proxy, false, callback._remote, message, message_content, nil)
	return false
end

if id_servers_list_message_handler ~= nil 
then proxy_handlers:Remove('ServersListMessage', id_servers_list_message_handler) end

id_servers_list_message_handler = proxy_handlers:Add('ServersListMessage', servers_list_message)