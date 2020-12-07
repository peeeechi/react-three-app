
export type WebsocketState = "connecting"|"close"|"error";
export type ConnectionChangedCallback = (state: WebsocketState) => void;
export type ReceiveMessageCallback = (e: MessageEvent) => void;


export default class WebSocketClient {
    constructor() {
    }

    
    private _websocketState : WebsocketState = "close";

    public get websocketState() : WebsocketState {
        return this._websocketState;
    }

    private setWebsocketState = (v : WebsocketState) => {
        this._websocketState = v;
        this.ConnectionStateChangedEvent.forEach(callback => {
            callback(v);
        });
    }
    
    public send = (message: string) => {
        if (this.websocketState == "connecting") {
            this.client?.send(message);
        }
    }

    public connect = (uri: string) => {

        if (this.client !== null && this.websocketState == "connecting") {
            return;
        }
        
        this.client = new WebSocket(uri);
        
        // 接続時処理
        this.client.onopen = this.onWebSocketConnecting;
        
        // 切断時処理
        this.client.onclose = this.onWebSocketClosing;
        
        this.client.onerror = this.onWebSocketError;
        
        this.client.onmessage = (ev) => {
            this.MessageReceivedEvent.forEach(callback => {
                callback(ev);
            });
        };
    }
    
    public Disconnect = () => {
        if (this.client == null || this.websocketState != "connecting") {
            return;
        }

        this.client.close();
        this.client = null;
    }

    /**
     * WebSocket 接続時処理
     */
    private onWebSocketConnecting = (ev: Event) => {
        console.log('websocket: open', ev);
        this.setWebsocketState("connecting");
    }

    /**
     * WebSocket 切断時処理
     */
    private onWebSocketClosing = (ev: CloseEvent) => {
        console.log('websocket: closed', ev);
        this.setWebsocketState("close");
    }

    /**
     * WebSocket 切断時処理
     */
    private onWebSocketError = (ev: Event) => {
        console.log('websocket: has error', ev);
        this.setWebsocketState("close");
    }

    
    private client: WebSocket|null = null;    
    public ConnectionStateChangedEvent: ConnectionChangedCallback[] = [];
    public MessageReceivedEvent: ReceiveMessageCallback[] = [];
    
    public get Url(): string | null {
        return this.client === null? null : this.client!.url;
    }
}
