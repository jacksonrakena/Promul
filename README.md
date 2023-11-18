# Promul
Using the [LibNetLib](https://github.com/RevenantX/LiteNetLib) reliable UDP library, Promul is a simple, lightweight, and easy-to-use relay server implementation for [Unity Netcode for GameObjects](https://docs-multiplayer.unity3d.com/netcode/current/about/). 
  
Promul serves as a free and open-source alternative to [Unity Relay](https://unity.com/products/relay). In the future, we would like to add automatic provisioning of Promul instances behind a REST API, including a secret join code mechanism, to bring Promul into feature parity with Unity Relay.

## Setup
#### Relay server
Clone this repository and build and run the program under [/Server](/Server). Promul will bind on port 4098.

#### Relay client (Transport)
Install the transport by adding it in the Unity Package Manager by git URL:  
`https://github.com/jacksonrakena/Promul.git`
  
Next, add `PromulTransport` to your NetworkManager GameObject. Set the `Address` and `Port` to the address and port of your relay server.  

Example:  
![Alt text](example.png)  
  

You should now be ready to use Promul as a relay server for your Unity project. You can call `NetworkManager`'s `StartHost`/`StartClient`/`StartServer` methods as normal.

## Engineering
### Protocol
Promul uses its own wire protocol over LibNetLib. The protocol is shown in the following diagram:
```mermaid
sequenceDiagram
    autonumber
    Host (ID 0)->>+Relay: UDP Connect
    Client (ID 1)->>+Relay: UDP Connect
    Relay->>+Host (ID 0): 0x11 CLIENT_CONNECTED
    Relay->>-Client (ID 1): 0x10 CONNECTED
    rect rgba(0, 0, 255, .1)
            note right of Relay: Data is now relayed by 0x00 DATA messages
    Client (ID 1)->>+Relay: 0x00 DATA, target=0
    Relay-->>+Host (ID 0): 0x00 DATA, author=1
    Host (ID 0)->>+Relay: 0x00 DATA, target=1,
    Relay->>+Client (ID 1): 0x00 DATA, author=0
    note right of Host (ID 0): author field becomes target when sending to relay
    end
    Client (ID 1)--xRelay: Connection lost
    Relay->>+Host (ID 0): 0x12 CLIENT_DISCONNECTED
    note right of Relay: Relay notifies host of disconnection
```
## Copyright
&copy; 2023 Firework Eyes Studio (NZBN 9429048922678) under the MIT License.
