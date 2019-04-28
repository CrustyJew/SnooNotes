import Vue from 'vue';
import Vuetify from 'vuetify';
import 'vuetify/dist/vuetify.min.css';

Vue.use(Vuetify, {
  theme: {
    primary: '#46d160',
    secondary: '#5F53C3',
    accent: '#DD1A0B',
    error: '#DD1A0B',
    info: '#2196F3',
    success: '#46D160',
    warning: '#FFF055',
  },
  customProperties: true,
  iconfont: 'md',
});
