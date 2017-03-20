<template>
    <modal :show.sync="show" :on-close.sync="close">
        <div class="modal-body">
        <h1>test</h1>
        </div>
    </modal>
</template>
<script>
import SNModal from './snModal.vue';
export default {
    components:{'modal': SNModal},
    props: ['show','onClose'],
    methods:{
        close(){
            this.onClose();
        }
    },
    name:'sn-options-modal'
}
</script>