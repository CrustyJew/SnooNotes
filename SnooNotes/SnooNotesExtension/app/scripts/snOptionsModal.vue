<template>
    <modal :show.sync="show" :on-close.sync="close">
        <div class="modal-body" v-if="!snOptions.loading">
        <h1>test</h1>
        <ul>
            <li v-for="sub in snOptions.inactiveSubs">{{sub}}</li>
            </ul>
        </div>
    </modal>
</template>
<script>
import SNModal from './snModal.vue';
export default {
    components:{'modal': SNModal},
    props: ['show','onClose', 'snOptions'],
    methods:{
        close(){
            this.onClose();
        }
    },
    name:'sn-options-modal'
}
</script>