import * as signalR from "@aspnet/signalr";
import { signalrBaseUrl } from '../config';

const connection = new signalR.HubConnectionBuilder().withUrl(signalrBaseUrl).build();