import * as Consul from "consul";
import RegisterOptions = Consul.Agent.Service.RegisterOptions;
import Service = Consul.Agent.Service;

import {IBusConfig, IConsumerDispose, RabbitHutch} from "easynodeq";
import {NewOrderMessage} from "./messages/newOrderMessage";
import {ShippingCreatedMessage} from "./messages/shippingCreatedMessage";
import * as uuid from 'node-uuid';

import * as restify from 'restify';
import StatusController from './controllers/statusController';
import {settings} from './config/settings';
import {bodyParser, fullResponse, queryParser} from 'restify';

console.log('Something works...')
