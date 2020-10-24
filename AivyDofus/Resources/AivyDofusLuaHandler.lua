character = {
	context = 0,
	mapKey = '',
	gameMapInfo = GameMapInformations()
}
-- HANDLER FORM --

-- AbstractClientReceiveCallback * NetworkElement * NetworkContentElement -> bool
-- function your_handler_function(AbstractClientReceiveCallback, NetworkElement, NetworkContentElement) 				-- creating a handler function
--		return true 																									-- return true if message handled will be forwarded else false (if false you'll have to send the msg manualy)
-- end
-- handler_id = handlers:Add('YourMessageHandlerName', your_handler_function)										 	-- adding handler to handlers list (use handler_id to remove from handlers)

-- END HANDLER FORM --

-- AbstractClientReceiveCallback * NetworkElement * NetworkContentElement -> bool 										-- return forwarding rcv data 
-- same as c# handler , check README and c# source code for more information
function protocol_required(callback, message, message_content)
	print('from lua : '..message.BasicString)
	return true
end

function hello_connect_message(callback, message, message_content)
	print('from lua : '..message.BasicString)
	return true
end

function map_complementary_informations_data_message(callback, message, message_content)
	print('from lua : '..message.BasicString)
	
	character.gameMapInfo:Clear()
	character.gameMapInfo.MapId = message_content['mapId']
	
	visualizer:FromDofusMap(character.gameMapInfo)

	return true
end

function game_map_movement_request_message(callback, message, message_content) -- from client
	print('from lua : '..message.BasicString)
	
	character.gameMapInfo:Clear()
	character.gameMapInfo.MapId = message_content['mapId']
	
	local keys = message_content['keyMovements']	
	local last_key = keys[keys.Length - 1]
	local first_key = keys[0]
	
	local start_cell = first_key & 4095
	local start_dir = (first_key >> 12) & 7
	
	local last_cell = last_key & 4095
	local last_dir = (last_key >> 12) & 7
	
	--local d_path = Pathfinder()
	--d_path:SetMap(character.gameMapInfo, true)
	--local cell_l = d_path:GetCompressedPath(start_cell, last_cell)
		
	--local i = 0
	--while i < cell_l.Length	do 
		--print(cell_l[i])
		--i = i + 1
	--end	
	visualizer:DrawPath({start_cell, last_cell})
	return true
end	

if id_protocol_required_handler ~= nil then proxy_handlers:Remove('ProtocolRequired', id_protocol_required_handler) end
if id_hello_connect_message_handler ~= nil then proxy_handlers:Remove('HelloConnectMessage', id_hello_connect_message_handler) end
if id_map_complementary_informations_data_message_handler ~= nil then proxy_handlers:Remove('MapComplementaryInformationsDataMessage', id_map_complementary_informations_data_message_handler) end
if id_game_map_movement_request_message_handler ~= nil then proxy_handlers:Remove('GameMapMovementRequestMessage', id_game_map_movement_request_message_handler) end

id_protocol_required_handler = proxy_handlers:Add('ProtocolRequired', protocol_required)
id_hello_connect_message_handler = proxy_handlers:Add('HelloConnectMessage', hello_connect_message)
id_map_complementary_informations_data_message_handler = proxy_handlers:Add('MapComplementaryInformationsDataMessage', map_complementary_informations_data_message)
id_game_map_movement_request_message_handler = proxy_handlers:Add('GameMapMovementRequestMessage', game_map_movement_request_message)

print('default lua handler initied')