import {THREE_AREA_ID} from './constant';
import * as three from 'three';
import ThreeApi from './three-api';

const threeApi = new ThreeApi(THREE_AREA_ID, new three.Color(0x111111));

export {
    threeApi,
};