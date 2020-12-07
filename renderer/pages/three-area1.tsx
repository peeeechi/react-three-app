import React from 'react';
import {THREE_AREA_ID_1} from '@/utils/constant';
import * as three from 'three';
// import {threeApi} from '@/utils/instance-store';
import WebSocketClient from '@/utils/websocket-client';
import ThreeApi, {loadGLTF, deg2rad, changeMaterial} from '@/utils/three-api';
import {WS_WEBSOCKET_CONTROLLER_URL_1, WS_WEBSOCKET_CONTROLLER_TEST_URL_1} from '@/utils/constant';
import NormalButton from '@/components/Button';
import ListTable from '@/components/ListTable';
// import PlotArea from '@/components/PlotArea';
// import RealtimeChart from '@/components/RealtimeChartPlotly';
import RealtimeChart from '@/components/RealTimeChartThree';
import "@/interfaces";
import { ForceSensorResponseSingle, ForceSensorData } from '@/interfaces/force-sensor-response';

const threeApi = new ThreeApi(THREE_AREA_ID_1, new three.Color(0x111111));

const test = false;

const URL = test? WS_WEBSOCKET_CONTROLLER_TEST_URL_1 : WS_WEBSOCKET_CONTROLLER_URL_1;

const plotMaxLength = 1000;

type ForceData = {
    "fx": number[],
    "fy": number[],
    "fz": number[],
}

type MomentData = {
    "mx": number[],
    "my": number[],
    "mz": number[],
}

type ThreeAreaState = {
    forceSensorResponse: ForceSensorResponseSingle,
    sensor1PlotDataForce: ForceData,
    sensor1PlotDataMoment: MomentData,
    logCount: number,
    isMoment: boolean,
};

class ThreeAreaPage extends React.Component<any, ThreeAreaState>{

    public forceSensorClient: WebSocketClient|null = null;
    private connectionKeepLoopId: number|null = null;

    private logBuffer: ForceSensorResponseSingle[]    = [];
    private isLogging: boolean                  = false;

    private sensor1ForceArrow: three.ArrowHelper = new three.ArrowHelper(
        new three.Vector3(0,0,1), /* dir */
        new three.Vector3(0,0,0), /* origin */
        0, /* length */
        0xffff55, /* color */
        0.2,
        0.2
    );

    constructor(props: any) {
        super(props);

        this.state = {
            forceSensorResponse: {
                force: {
                    "sensor-1": {fx:0,fy:0,fz:0,mx:0,my:0,mz:0} as ForceSensorData,
                    "time": 0,
                },
                robot: {
                    tcp: {
                        x: 0,
                        y: 0,
                        z: 0,
                        role: 0,
                        pitch: 0,
                        yaw: 0,
                    },
                    time: 0,
                },
                time: 0,
            },
            logCount: 0,
            isMoment: false,
            sensor1PlotDataForce: {
                fx:[],
                fy:[],
                fz:[],
            },
            sensor1PlotDataMoment: {
                mx: [],
                my: [],
                mz: [],
            },
        };
    }


    private websocketConnectionKeep = () => {
        if(this.forceSensorClient == null)
            return;

        switch (this.forceSensorClient.websocketState) {
            case "close":
            case "error":
                this.forceSensorClient.connect(URL);
                break;
            case "connecting":
            default:
                // 何もしない
                break;
        }
    }


    /**
     * コンポーネントがマウントされた時の処理
     */
    componentDidMount() {
        console.log("did mounted");

        this.forceSensorClient = new WebSocketClient();
        this.forceSensorClient.ConnectionStateChangedEvent.push((state) => {
            // alert(state);
        });

        this.forceSensorClient.MessageReceivedEvent.push((event) => {
            const resJson = event.data;
            const res = JSON.parse(resJson) as ForceSensorResponseSingle;
            // console.log(res);
            this.setState({forceSensorResponse: res});
            this.test(res);
        });

        this.connectionKeepLoopId = window.setInterval(this.websocketConnectionKeep, 1000);

        threeApi.init();

        const addPLight = (x:number, y:number, z:number) => {

            const pLight        = new three.PointLight("#ffffff", 1);
            pLight.castShadow   = true;
            pLight.position.set(x, y, z);

            threeApi.addObject(pLight);
        };

        const lightLength = 50;

        addPLight(0,0,lightLength);
        addPLight(0,0,-lightLength);
        addPLight(lightLength,lightLength,0);
        addPLight(-lightLength,lightLength,0);
        addPLight(lightLength,-lightLength,0);
        addPLight(-lightLength,-lightLength,0);

        threeApi.addObject(this.sensor1ForceArrow);
        loadGLTF("minebea-force-sensor.glb").then(gltf => {
        // loadGLTF("test.glb").then(gltf => {

            changeMaterial(gltf.scene.children, 0.3);

            const sensor1Group = gltf.scene;
            sensor1Group.position.z -= 1.5;

            threeApi.addObject(this.sensor1ForceArrow);
            threeApi.addObject(sensor1Group);
            threeApi.setTrackball(threeApi.scene.position);
        }).catch(error => {
            console.error('gltf load error: ', error);
        });
    }

    componentWillUnmount() {
        if(this.connectionKeepLoopId != null) {
            window.clearInterval(this.connectionKeepLoopId);
            this.connectionKeepLoopId = null;
        }
        threeApi.disposeOrbit();
        threeApi.disposeTrackball();

        if (this.forceSensorClient != null ) {
            if (this.forceSensorClient.websocketState == "connecting") {
                this.forceSensorClient.Disconnect();
            }
            this.forceSensorClient = null;
        }
    }

    recToggle = () => {
        // if(!this.isLogging) this.dataClear();

        if (this.forceSensorClient?.websocketState != "connecting") {
            return;
        }

        const args = this.isLogging? { cmd: "log-stop", args: `${Date.now()}-log.csv`,} : { cmd: "log-start", args: null,};

        this.forceSensorClient.send(JSON.stringify(args));

        this.isLogging = !this.isLogging;
    }

    dataClear = () => {
        this.logBuffer = [];
        this.setState({logCount: 0});
    }

    reauestReset = () => {
        if (this.forceSensorClient?.websocketState != "connecting") {
            return;
        }

        const args = { cmd: "sensor-reset", args: null,}

        this.forceSensorClient.send(JSON.stringify(args));
    }

    saveData = () => {

        if (!this.isLogging) {
            return;
        }

        // todo: send log-stop

    }
    saveAndDounload = (data: string, fileName: string) => {
        // const bom = new Uint8Array([0xEF, 0xBB, 0xBF]);
        // const blob = new Blob([ bom, content ], { "type" : "text/csv" });

        const blob = new Blob([data], { "type" : "text/csv" });

        if (window.navigator.msSaveBlob) {
            window.navigator.msSaveBlob(blob, fileName);

            // msSaveOrOpenBlobの場合はファイルを保存せずに開ける
            window.navigator.msSaveOrOpenBlob(blob, fileName);
        } else {

            const url = window.URL.createObjectURL(blob);
            const a = document.createElement("a");
            document.body.appendChild(a);
            a.download = fileName;
            a.href = url;
            a.click();
            a.remove();
            window.URL.revokeObjectURL(url);
        }

    }

    test(response: ForceSensorResponseSingle) {

        if (this.isLogging) {
            this.logBuffer.push(response);
            this.setState({logCount: this.logBuffer.length});
        }

        const setArrow = (data: ForceSensorData | null, target: three.ArrowHelper) => {
            const dir = new three.Vector3(0,0,0);
            if (data != null) {
                const pos = this.state.isMoment? new three.Vector3(data.mx*100, data.my*100, data.mz*100) :new three.Vector3(data.fx, data.fy, data.fz) ;
                dir.x = pos.x;
                dir.y = pos.y;
                dir.z = pos.z;
            }


            target.setDirection(dir);
            target.setLength(dir.length() * 10);
        }

        if (response.force["sensor-1"] != null) {
            const data = response.force["sensor-1"];
            const srcForce  = Object.assign({}, this.state.sensor1PlotDataForce);
            const srcMoment = this.state.sensor1PlotDataMoment;

            srcForce.fx.push(data.fx);
            if (srcForce.fx.length > plotMaxLength) srcForce.fx.shift();
            srcForce.fy.push(data.fy);
            if (srcForce.fy.length > plotMaxLength) srcForce.fy.shift();
            srcForce.fz.push(data.fz);
            if (srcForce.fz.length > plotMaxLength) srcForce.fz.shift();

            srcMoment.mx.push(data.mx);
            if (srcMoment.mx.length > plotMaxLength) srcMoment.mx.shift();
            srcMoment.my.push(data.my);
            if (srcMoment.my.length > plotMaxLength) srcMoment.my.shift();
            srcMoment.mz.push(data.mz);
            if (srcMoment.mz.length > plotMaxLength) srcMoment.mz.shift();

            this.setState({
                sensor1PlotDataForce: srcForce,
                sensor1PlotDataMoment: srcMoment,
            });
        }
        // console.log('sensor1 force length:', this.state.sensor1PlotDataForce.fx.length);
        setArrow(response.force["sensor-1"], this.sensor1ForceArrow);
    }

    toggleChange = () => {
        this.setState({isMoment: !this.state.isMoment});
    }

    render() {
        return (
            <>
                <div className="grid grid-rows-3 grid-cols-12 w-full h-full">

                            <div className="row-span-2 col-span-6 bg-gray-200">
                                <canvas className="w-full h-full" id={THREE_AREA_ID_1}></canvas>
                            </div>

                            <div className="row-span-2 col-span-6 bg-green-100">
                                <div className="grid grid-rows-2 w-full h-full">
                                    <RealtimeChart canvasId="plot1" displayCount={1000} rangeX={20} rangeY={20} watchList={
                                        [
                                            {data: this.state.forceSensorResponse.force["sensor-1"]!.fx, name: "sensor1-fx"},
                                            {data: this.state.forceSensorResponse.force["sensor-1"]!.fy, name: "sensor1-fy"},
                                            {data: this.state.forceSensorResponse.force["sensor-1"]!.fz, name: "sensor1-fz"},
                                        ]
                                    }></RealtimeChart>
                                    <RealtimeChart canvasId="plot2" displayCount={1000} rangeX={20} rangeY={20} watchList={
                                        [
                                            {data: this.state.forceSensorResponse.force["sensor-1"]!.mx * 1000, name: "sensor1-mx"},
                                            {data: this.state.forceSensorResponse.force["sensor-1"]!.my * 1000, name: "sensor1-my"},
                                            {data: this.state.forceSensorResponse.force["sensor-1"]!.mz * 1000, name: "sensor1-mz"},
                                        ]
                                    }></RealtimeChart>

                                    {/* <div className="bg-blue-600"></div> */}
                                    {/* <div className="bg-red-600"></div> */}

                                </div>
                            </div>


                            <div className="col-span-2">
                                <h3>sensor 1</h3>
                                <ListTable obj={this.state.forceSensorResponse.force["sensor-1"]}/>
                            </div>
                            <div className="col-span-2">
                                <h3>tcp</h3>
                                <ListTable obj={this.state.forceSensorResponse.robot.tcp}/>
                            </div>

                            <div className="col-span-2">
                                <div className="flex flex-col justify-center">

                                    <p>{`log count: ${this.state.logCount}`}</p>
                                    <NormalButton className="m-2" colorType="blue" onClick={this.toggleChange}>{this.state.isMoment? "Show force" : "Show moment"}</NormalButton>
                                    <NormalButton className="m-2" colorType="blue" onClick={this.recToggle}>{this.isLogging? "Stop": "Start"}</NormalButton>
                                    <NormalButton className="m-2" colorType="blue" onClick={this.reauestReset}>Reset sensor value</NormalButton>
                                </div>
                            </div>

                </div>
            </>
        );
    }
}

export default ThreeAreaPage;